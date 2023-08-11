import matplotlib.pyplot as plt
import numpy as np


def ReadCsv(path):
    np.set_printoptions(suppress=True)
    features = np.genfromtxt(path, delimiter=';')
    return features[1:, 1:5].astype('float')


def Plot(data):
    plt.figure(1)
    plt.style.use("ggplot")

    x = np.arange(1, 31)
    plt.plot(x, data[:, 0],   label='BestFitness')
    plt.plot(x, data[:, 1],   label='AverageFitnessPopulation')
    plt.plot(x, data[:, 2]+2, label='BestMaxDistance')
    plt.plot(x, data[:, 3],   label='BestMaxDistanceTime')

    plt.legend(loc="upper left")
    plt.xlabel('Generations')
    plt.ylabel('Values')

    plt.show()


d = ReadCsv("./EvolutionLog.csv")
Plot(d)

