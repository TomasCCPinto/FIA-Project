using System.Collections;
using System.Collections.Generic;
using GeneticSharp.Runner.UnityApp.Car;
using GeneticSharp.Runner.UnityApp.Commons;
using UnityEngine;

public static class GeneticAlgorithmConfigurations
{
    /* YOUR CODE HERE:
    * 
    * Configuration of the algorithm: You should change the configurations of the algorithm here
    */

    public static float crossoverProbability = 0.9f; 
    public static float mutationProbability = 0.3f; 
    public static int maximumNumberOfGenerations = 30; 
    public static int eliteSize = 2; 

    public static UniformCrossover crossoverOperator = new UniformCrossover(crossoverProbability);
    public static GaussianMutation mutationOperator = new GaussianMutation();
    public static Roulette parentSelection = new Roulette();
    public static Elitism survivorSelection = new Elitism(eliteSize);
    public static GenerationsTermination terminationCondition = new GenerationsTermination(maximumNumberOfGenerations);
}
