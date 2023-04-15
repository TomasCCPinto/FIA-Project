using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Randomizations;
using System;

public class GaussianMutation : IMutation
{
    public bool IsOrdered { get; private set; } // indicating whether the operator is ordered (if can keep the chromosome order).
    private double std = 1;

    public GaussianMutation(double std)
    {
        IsOrdered = true;
        this.std = std;

    }

    public GaussianMutation()
    {
        IsOrdered = true;

    }

    public void Mutate(IChromosome chromosome, float probability)
    {
        //YOUR CODE HERE

    }

    protected double SampleGaussian(double mean, double std)
    {
        //Generates a random number based on a normal distribution using the Box-Muller method.
        double x1 = RandomizationProvider.Current.GetDouble(0, 1);
        double x2 = RandomizationProvider.Current.GetDouble(0, 1);
        double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
        return y1 * std + mean;

    }

}
