from includes.imports import *

class StringOp():
    @staticmethod
    def clean(doc):
        '''
        cleans the sentences
        :param doc: The raw text file
        :return: a list of cleaned sentences
        '''
        def one_xlat(match):
            return replace_dict[match.group(0)]
        t = re.sub("[\(\[].*?[\)\]]", "[FILLER]", doc)
        replace_dict = {'- ': '', '     ': ' ', '\n': ' ','/':' ','-':' ','!':'','*':'','[':'',']':'','(':'',')':'','%':'','?':'',';':'','+':'','~':'','"':'','#':'','_':'', '\t':' ',"\\":'',">":'','<':'', '. .': '.'}
        first_filter = {'Dr. ':'', 'Mr. ':'', 'Ms. ':'', 'Mrs. ':'', 'No Known Allergies':'', 'History:': '', 'Adverse Drug Reactions': '', 'Birth:': '', 'IMPRESSION:':'',  'Sex:': '', 'Admission Date:': '', 'Discharge Date:': '', 'Chief Complaint': '', 'Service:': '', 'Allergies:': '', 'Attending:': '', 'Major Surgical or Invasive Procedure:': '', 'History of Present Illness:': '', 'Past Medical History:': '', 'Discharge Instructions:': '', 'Followup Instructions:': '', 'Sig:': '', 'Family History:': '', 'Physical Exam:': '', 'Tablet': '', 'Sodium': '', 'Inhalation': '', 'Capsule': '', 'Condition:': '', 'Facility:': '', 'Release': '', 'Allergies': ''}

        for k,v in first_filter.items():
            t = t.replace(k, v)
        res = re.finditer("[(\n|. )]([A-Z][a-z])", t) #[(\n|. )][A-Z][a-z]
        laststart = len(t)
        sentences = []

        elements = [m for m in res]
        for i in range(len(elements)-1, -1, -1):
            m = elements[i]
            newstart = m.start(0)
            sentence = t[newstart:laststart].replace('\n', ' ')
            rx = re.compile('|'.join(map(re.escape, replace_dict)))
            sentence = rx.sub(one_xlat, sentence)
            
            if len(sentence.split(' '))>3:
                sentence = sentence.replace(' .', '')
                sentence = sentence.replace('. ', '')
                sentence = sentence.replace('.', '')
                sentences.append(sentence[1:] + '.')

            laststart = newstart
        return sentences

    @staticmethod
    def find_condition(wordlist, text):
        '''
        find condition and the their positions in the text
        :param wordlist: a list of synonym conditions
        :param text: raw text
        :return: a dict mapping conditions to a list of their positions
        '''
        text = text.lower()
        results = {}
        for lst in wordlist:
            prime_specifier = lst[0]
            for entry in lst: #look for all synonyms
                if entry in text:
                    blanks = entry.split(' ')
                    baseindices = [match.start() for match in re.finditer(re.escape(entry), text)]
                    for baseindex in baseindices:
                        pos_char = []
                        ct = baseindex
                        for entry in blanks:
                            pos_char.append(ct)
                            ct+=len(entry)+1
                        if not prime_specifier in results: results[prime_specifier] = set()
                        results[prime_specifier].update(pos_char)
        return results