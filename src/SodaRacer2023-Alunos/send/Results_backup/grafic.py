#!/usr/bin/env python3

import matplotlib.pyplot as plt
import numpy as np
from os import listdir
from os.path import isfile, join
from functools import cmp_to_key


def readCsv(path):
    np.set_printoptions(suppress=True)
    features = np.genfromtxt(path, delimiter=';')
    return features[1:, 1:5].astype('float')


def plot(data, fileName):
    plt.figure(figsize=(18, 12))
    # plt.figure()
    plt.style.use("ggplot")

    x = np.arange(1, 31)
    plt.plot(x, data[:, 0],   label='BestFitness')
    plt.plot(x, data[:, 1],   label='AverageFitnessPopulation')
    plt.plot(x, data[:, 2]+2, label='BestMaxDistance')
    plt.plot(x, data[:, 3],   label='BestMaxDistanceTime')

    plt.legend(loc="upper left")
    plt.xlabel('Generations')
    plt.ylabel('Values')

    plt.savefig(fileName, dpi=300, bbox_inches='tight')


def compare(a, b):
    if len(a) < len(b):
        return True
    elif len(a) > len(b):
        return False
    return a[27:] <= b[27:]


def readDirectory(path):
    dirnames = [f for f in listdir(path) if not isfile(join(path, f))]
    # for i in range(len(dirnames)):
    #     dirnames[i] = dirnames[i][27:]

    sorted(dirnames, key=cmp_to_key(compare))
    # dirnames.sort()

    for name in dirnames:
        print(name)
    
    return
    i = 0
    for dir in dirnames:
        pathToCsv = path + "/" + dir + "/EvolutionLog.csv"
        data = readCsv(pathToCsv)
        
        plot(data, dir + "_" + str(i) + ".png")
        i += 1



if __name__ == '__main__':
    # d = ReadCsv("./EvolutionLog.csv")
    # Plot(d)
    readDirectory(".")

