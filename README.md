# Medical Record Search Engine

Authors: Vincent Casser, Shiyu Huang, Filip Michalsky

A search engine designed specifically for medical records. The work was done in cooperation with the Center for Clinical Data Science (CCDS), a joint effort of MGH and BWH. This repository is the final delivered product.

## steps to run the software:

1. git clone 

2. open the [index/DownloadFiles.ipynb](index/DownloadFiles.ipynb), paste password, run the notebook to retrieve pre-computed index files and models

3. (Optional) to index medical report from scratch, run [RunIndexing.ipynb](RunIndexing.ipynb)

4. (Optional) to retrain both Word2Vec models from scratch, run [RunW2VecTraining.ipynb](RunW2VecTraining.ipynb)

5. Run [Backend.ipynb](Backend.ipynb) to start query server

The frontend application is written in Visual Basic.NET. The Visual Studio Project files and executables can be found in the frontend folder.

## Dependency Requirements
- pandas

- spacy (install the English language package by running command ```python -m spacy download en```)

- numpy

- matplotlib

- sklearn

- gensim

- fasttext

- tqdm

- glob

- urllib3

- nltk (install wordnet by running the python code ```nltk.download('wordnet')```)

