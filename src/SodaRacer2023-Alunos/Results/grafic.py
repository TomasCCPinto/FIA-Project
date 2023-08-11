#!/usr/bin/env python3

import matplotlib.pyplot as plt
import numpy as np

from os import listdir
from os import getcwd 
from os.path import isfile, join
import warnings

warnings.simplefilter("ignore", category=RuntimeWarning)


def readCsv(path):
    np.set_printoptions(suppress=True)
    features = np.genfromtxt(path, delimiter=';')
    return features[1:, 1:-1].astype('float')


def plot(data, fileName):
    # plt.figure(figsize=(18, 12))
    plt.figure()
    plt.style.use("ggplot")
    plt.rcParams.update({'font.size': 5})

    for i in range(len(data)):
        x = np.arange(1, int(np.shape(data[i])[0])+1)
        print(np.shape(x))
        print(np.shape(data[i][:, 0]))

        plt.plot(x, data[i][:, 0], label='BestFitness ' + str(i))
        plt.plot(x, data[i][:, 1], label='AverageFitnessPopulation' + str(i))

    plt.legend(loc="upper left")
    plt.xlabel('Generations')
    plt.ylabel('Fitness Values / log_10')
    plt.yscale("log")
    plt.savefig(fileName + getcwd()[-1] + ".png", dpi=300, bbox_inches='tight')

    plt.figure()
    plt.rcParams.update({'font.size': 5})

    for i in range(len(data)):
        x = np.arange(1, int(np.shape(data[i])[0])+1)
        avg = np.zeros(len(data[i][:, 0]))
        std = np.zeros(len(data[i][:, 0]))

        for j in range(len(data[i][:, 0])):
            avg[j] = np.average(data[i][:j, 0])
            std[j] = np.std(data[i][:j, 0])

        plt.plot(x, avg, label='Average' + str(i))
        plt.plot(x, std, label='standardDeviation' + str(i))

    plt.legend(loc="upper left")
    plt.xlabel('Generations')
    plt.ylabel('Fitness Values')
    plt.yscale("log")
    plt.savefig(fileName + getcwd()[-1] + "1.png", dpi=300, bbox_inches='tight')


def readDirectory(path):
    dirnames = [f for f in listdir(path) if not isfile(join(path, f))]

    data = []
    for dir in dirnames:
        pathToCsv = path + "/" + dir + "/EvolutionLog.csv"
        data.append(readCsv(pathToCsv))
        
    plot(data, "file")


if __name__ == '__main__':
    # d = ReadCsv("./EvolutionLog.csv")
    # Plot(d)
    readDirectory(".")

