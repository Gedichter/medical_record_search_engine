from includes.imports import *

class logger(gensim.models.callbacks.CallbackAny2Vec): 
    def __init__(self):
        self.epoch = 0
    def on_epoch_begin(self, model):
        print("Epoch #{} start".format(self.epoch))
    def on_epoch_end(self, model):
        print("Epoch #{} end".format(self.epoch))
        self.epoch += 1

class W2VecModel:
    model = None
    
    def load_model(self, file):
        '''
        loads a pre-trained word2vec model from file.
        file: filename to read.
        '''
        self.model = gensim.models.fasttext.FastText.load(file)
        
    def get_vocab(self):
        '''
        returns the vocabulary in the model.
        '''
        wstr = []
        for key in self.model.wv.vocab:
            wstr.append(str(key))
        return wstr

    def get_vec(self, word):
        '''
        returns the vector representation for word.
        '''
        assert word in self.model.wv, 'word ' + str(word) + ' not found in vocab.'
        return self.model.wv[word]

    def get_similarity(self, word1, word2):
        '''
        returns the similarity between word1 and word2.
        word1: first word.
        word2: second word.
        '''
        return self.model.wv.similarity(word1, word2)

    def get_extreme_similarities(self, word, top=10, highest=True):
        '''
        returns the most (or least) similar words in the whole vocabulary to word.
        word: the word for which we want to find the closest words.
        top: number of elements to return in the ranked list.
        highest: determines if we look for the most or least similar words.
        '''
        ddict = {}
        for key in self.model.wv.vocab:
            ddict[key] = self.model.wv.similarity(word, key)
        ddict = sorted(ddict.items(), key=operator.itemgetter(1), reverse=highest)
        return ddict[0:top]

    def rank_words(self, query, ref_words):
        '''
        ranks the finite collection of given words according to similarity to query word.
        query: the word to compare to.
        ref_words: the words which should be ranked according to similarity to query.
        '''
        ddict = {}
        for w in ref_words:
            ddict[w] = self.model.wv.similarity(query, w)
        ddict = sorted(ddict.items(), key=operator.itemgetter(1), reverse=True)
        return ddict

    def cosine_sim(self, x, y):
        '''
        computes the cosine similarity between vectors x and y.
        x: first vector.
        y: second vector.
        '''
        return np.dot(x, y) / (np.linalg.norm(x, ord=2) * np.linalg.norm(y, ord=2))
    
    def perform_PCA(self, ref_word=None, top=10):
        '''
        performs PCA on the word vectors in the model.
        ref_word: if this is set to a word in the vocab, we highlight the most similar words to it in the plot.
        top: determines how many of the most similar words to ref_word to display.
        '''
        d = decomposition.PCA(2)
        X = np.zeros([len(self.model.wv.vocab), self.model.vector_size])
        ct = 0
        for key in self.model.wv.vocab:
            X[ct] = self.model.wv[key]
            ct += 1

        d.fit(X)
        print('explained variance ratio', d.explained_variance_ratio_)

        # overall scatterplot
        result = d.transform(X)
        res = plt.figure(figsize=(10, 7))
        res = plt.scatter(result[:, 0].reshape(-1, 1), result[:, 1].reshape(-1, 1), s=0.2)

        # highlight the top closest words to ref_word if applicable
        if ref_word != None:
            X2 = np.zeros([top, self.model.vector_size])
            ddict = self.get_extreme_similarities(ref_word, top=top)[:top]
            ct = 0
            for key, val in ddict:
                print(key, val)
                X2[ct] = self.model.wv[key]
                ct += 1
            result_selected = d.transform(X2)
            res = plt.scatter(result_selected[:, 0].reshape(-1, 1), result_selected[:, 1].reshape(-1, 1), s=2, c='red')
            res = plt.xlabel(d.explained_variance_ratio_[0])
            res = plt.ylabel(d.explained_variance_ratio_[1])

    def get_sentence_vec(self, sentence, standardize=False):
        '''
        returns the overall sentence sentiment.
        sentence: the input sentence.
        standardize: if set to true, we standardize with respect to the sentence length.
        '''
        totalvec = np.zeros(self.model.vector_size)
        for w in sentence:
            if w in self.model.wv:
                totalvec += self.model.wv[w]
        if standardize: totalvec = totalvec / len(sentence)
        return totalvec

    def get_sentence_similarity(self, sentence1, sentence2, standardize=False):
        '''
        computes the sentence similarity based on sentence sentiment.
        sentence1: the first sentence.
        sentence2: the second sentence.
        standardize: if set to true, we standardize with respect to the sentence length.
        '''
        s1 = self.get_sentence_vec(self.helper.clean_sentence(sentence1))
        s2 = self.get_sentence_vec(self.helper.clean_sentence(sentence2))
        return self.cosine_sim(s1, s2)

    def train_model(self, sentences, size, window, min_count, workers, max_vocab_size, use_char_gram, char_n_min, char_n_max, file):
        '''
        trains a word2vec model on the given sentences.
        for reference, documentation is available on https://radimrehurek.com/gensim/models/word2vec.html.
        size: size of vocabulary.
        window: size of neighborhood window.
        min_count: minimum number of occurrences to not ignore words.
        workers: number of threads to use.
        max_vocab_size: number of words in the training vocabulary
        use_char_gram: boolean flag, determine if use character n-gram based model
        char_n_min: min char number in the character n-gram model
        char_n_max: max char number in the character n-gram model
        file: store the model in the file
        '''
        if not use_char_gram:
            char_n_min=2
            char_n_max=1
        model = gensim.models.fasttext.FastText(sentences=sentences, sg=1, size=size, window=window,
                                       max_vocab_size=max_vocab_size, min_count=min_count, workers=workers, min_n=char_n_min, max_n=char_n_max, callbacks=[logger()])
        model.save(file)
        self.model = model