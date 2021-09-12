import glob
import os
import sys
#from logging import Logger
import logging
from scipy.spatial.ckdtree import ordered_pairs
sys.path.append(".")
from es_inserter import Inserter
#from logger.logger import Logger
import os, operator, sys

def setup_custom_logger(name):
    formatter = logging.Formatter(fmt='%(asctime)s %(levelname)-8s %(message)s',
                                  datefmt='%Y-%m-%d %H:%M:%S')
    handler = logging.FileHandler(name, mode='w')
    handler.setFormatter(formatter)
    logger = logging.getLogger(name)
    logger.setLevel(logging.INFO)
    logger.addHandler(handler)
    return logger

if __name__ == '__main__':
    all_files = glob.glob("D:\\yago\\tsv\\*.tsv")
    #dirpath = os.path.abspath("D:\\yago\\new_tsv")
    # make a generator for all file paths within dirpath
    ##all_files = (os.path.join(basedir, filename) for basedir, dirs, files in os.walk(dirpath) for filename in files)
    sorted_files = sorted(all_files, key=os.path.getsize, reverse=True)

    for file in sorted_files:
        head, tail = os.path.split(file)
        pre, ext = os.path.splitext(tail)
        logger = setup_custom_logger("c:\\log\\" + pre + ".log")
        es_inserter = Inserter(logger)
        es_inserter.insert(file, pre.lower())