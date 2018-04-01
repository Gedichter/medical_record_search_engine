# Medical Record Search Engine

Authors: Vincent Casser, Shiyu Huang, Filip Michalsky

A search engine designed specifically for medical records. The project was done in cooperation with the Center for Clinical Data Science (CCDS), a joint effort between MGH and BWH. This repository is the final deliverable.

The search engine's use case is looking up reports from a database based on a medical condition-specific query performed by either a machine learning engineer or a doctor. 

This search engine is built mainly on NLP library [spacy](https://spacy.io/).

## steps to run the software:

1. git clone 

2. open the [index/DownloadFiles.ipynb](index/DownloadFiles.ipynb), paste password, run the notebook to retrieve pre-computed index files and models

3. Paste MIMIC's data (NOTEEVENTS.csv) into [data/](data) folder.

4. (Optional) to index medical report from scratch, run [RunIndexing.ipynb](RunIndexing.ipynb)

5. (Optional) to retrain both Word2Vec models from scratch, run [RunW2VecTraining.ipynb](RunW2VecTraining.ipynb)

6. Run [Backend.ipynb](Backend.ipynb) to start query server

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

