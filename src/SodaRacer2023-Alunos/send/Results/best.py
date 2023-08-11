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
    # BestFitness;AverageFitnessPopulation;BestMaxDistance;BestMaxDistanceTime;BestNumberOfWheels;BestCarMass;BestIsRoadComplete
    i = 0
    for i in range(len(data)):
        plt.figure()
        plt.style.use("ggplot")
        plt.rcParams.update({'font.size': 5})

        x = np.arange(1, int(np.shape(data[i])[0])+1)
        print(np.shape(x))
        print(np.shape(data[i][:, 0]))

        # plt.plot(x, data[i][:, 0], label='BestFitness ' + str(i))
        # plt.plot(x, data[i][:, 2], label='BestMaxDistance' + str(i))
        # plt.plot(x, data[i][:, 3], label='BestMaxDistanceTime' + str(i))
        plt.plot(x, data[i][:, 4], label='BestNumberOfWheels' + str(i))
        plt.plot(x, data[i][:, 5], label='BestCarMass' + str(i))

        plt.legend(loc="upper left")
        plt.xlabel('Generations')
        plt.ylabel('Values')
        plt.savefig(fileName + str(i) + ".png", dpi=300, bbox_inches='tight')


def readDirectory(path):
    dirnames = [f for f in listdir(path) if not isfile(join(path, f))]

    data = []
    for dir in dirnames:
        pathToCsv = path + "/" + dir + "/EvolutionLog.csv"
        data.append(readCsv(pathToCsv))
        
    plot(data, "analise")


if __name__ == '__main__':
    # d = ReadCsv("./EvolutionLog.csv")
    # Plot(d)
    readDirectory(".")

