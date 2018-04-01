from includes.imports import *

class QueryExpander():
    def __init__(self, model_W2vec, model_Chargram):
        self.W2VEC = model_W2vec
        self.CHARGRAM = model_Chargram
    
    def expand_word(self, word, syn_filter=True, syn_threshold= 0, var_thres_sim= 5, var_thres_dis= -1):
        '''
        getting the synonyms and misspelling/variants list of the target word
        :param word: the target word
        :param syn_filter: whether use word2vec model to filter the synonyms
        :param syn_threshold: the threshold of the cosine similarity of two words to decide if they are synonyms
        :param var_thres_sim: threshold for the number of misspelling/variant candidates
        :param var_thres_dis: the threshold for edit distance to determine misspelling/variant. -1 means automatic threshold
        :return: the list of synonyms and variants with similarity measure
        '''
        if var_thres_dis<0:
            var_thres_dis = 2 if len(word)>4 else 1
        syn_result = []
        if syn_filter:
            syn_result = self.get_synonym(word, syn_threshold)
        else:
            syn_result = self.get_synonym_no_filtering(word)
        var_result = self.get_variants(word, var_thres_sim, var_thres_dis)
        return {**syn_result, **var_result, **{word: 1}} 
    
    def get_synonym(self, word, threshold):
        '''
        get the set of synonyms of a word(including itself)
        :param word: the target word
        :param threshold: the threshold of the cosine similarity in W2Vec model
        :return: list of synonyms with the similarity
        '''
        synonyms = {}
        vocab = self.W2VEC.get_vocab()
        for syn in wordnet.synsets(word):
            for l in syn.lemmas():  
                name = l.name()
                
                if name in vocab and not name in synonyms:
                    similarity = self.W2VEC.get_similarity(word, name)
                    if(similarity > threshold):
                        synonyms[name] = similarity
        return synonyms
    
    def get_synonym_no_filtering(self, word):
        '''
        get the set of synonyms of a word(including itself) purely from WordNet
        Similarity measure of all words default to 1
        :param word: target word
        :return: list of synonyms
        '''
        synonyms = {}
        for syn in wordnet.synsets(word):
            for l in syn.lemmas():   
                name = l.name()
                synonyms[name] = 1 
        return synonyms

    def minDistance(self, word1, word2):
        '''
        return the edit distance between word1 and word2, each edit can be one insert, delete or replace
        :param word1:
        :param word2:
        :return: the edit distance betweeen word1 and word2
        '''

        if len(word1) == 0:
            return len(word2)
        if len(word2) == 0:
            return len(word1)
        dp = [['#' for _ in range(len(word2)+1)] for _ in range((len(word1))+1)]

        for i in range(len(word1)+1):
            for j in range(len(word2)+1): 
                if i == 0:
                    dp[i][j] = j
                elif j == 0:
                    dp[i][j] = i
                elif word1[i-1] == word2[j-1]:                    
                    dp[i][j] = min(dp[i-1][j-1], dp[i-1][j]+1, dp[i][j-1]+1)
                else:
                    dp[i][j] = min(dp[i-1][j]+1, dp[i][j-1]+1, dp[i-1][j-1] + 1)
        return dp[len(word1)][len(word2)]

    def get_variants(self, word, thres_sim, thres_dis):
        '''
        return morphological variants (misspelling and variant) of a word
        :param word: target word
        :param thres_sim: threshold for the number of misspelling/variant candidates
        :param thres_dis: the threshold for edit distance to determine misspelling/variant. -1 means automatic threshold
        :return: list of variants with similarity measure
        '''
        candidates = self.CHARGRAM.get_extreme_similarities(word, thres_sim)
        result = {}
        for c in candidates:
            w, _ = c
            if self.minDistance(w, word) <= thres_dis:
                result[c[0]] = c[1]
        return result