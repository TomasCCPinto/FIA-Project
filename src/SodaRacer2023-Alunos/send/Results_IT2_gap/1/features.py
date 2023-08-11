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
    return features[1:, 1].astype('float'), features[1:, 3:-1].astype('float')


def plot(data, fileName):
    # plt.figure(figsize=(18, 12))
    plt.figure()
    plt.style.use("ggplot")
    plt.rcParams.update({'font.size': 5})

    for i in range(len(data)):
        x = np.arange(1, int(np.shape(data[0])[0])+1)
        plt.plot(x, data[i][:, 0], label='BestMaxDistance' + str(i))
        plt.plot(x, data[i][:, 1], label='BestMaxDistanceTime' + str(i))
        plt.plot(x, data[i][:, 2], label='BestNumberOfWheels' + str(i))
        plt.plot(x, data[i][:, 3], label='BestCarMass' + str(i))
        break

    plt.legend(loc="upper left")
    plt.xlabel('Generations')
    plt.ylabel('Values')
    plt.savefig(fileName + getcwd()[-1] + ".png", dpi=300, bbox_inches='tight')

    plt.figure()
    plt.rcParams.update({'font.size': 5})


def readDirectory(path):
    dirnames = [f for f in listdir(path) if not isfile(join(path, f))]

    data = []
    for dir in dirnames:
        pathToCsv = path + "/" + dir + "/EvolutionLog.csv"
        data.append(readCsv(pathToCsv))
        
    plot(data, "feature")


if __name__ == '__main__':
    # d = ReadCsv("./EvolutionLog.csv")
    # Plot(d)
    readCsv("./GapRoad-2023-27-4-23-43-22-crossProb-0.9-elite-2-mutProb-0.05")

