using GeneticSharp.Domain.Chromosomes;
using System;
using System.Linq;
using UnityEngine;
using GeneticSharp.Domain.Randomizations;
using System.Collections.Generic;
using GeneticSharp.Domain.Crossovers;

namespace GeneticSharp.Runner.UnityApp.Commons
{
    public class UniformCrossover : ICrossover
    {
  

        

        public int ParentsNumber { get; private set; }

        public int ChildrenNumber { get; private set; }

        public int MinChromosomeLength { get; private set; }

        public bool IsOrdered { get; private set; } // indicating whether the operator is ordered (if can keep the chromosome order).

        protected float crossoverProbability;


        public UniformCrossover(float crossoverProbability) : this(2, 2, 2, true)
        {
            this.crossoverProbability = crossoverProbability;
        }

        public UniformCrossover(int parentsNumber, int offSpringNumber, int minChromosomeLength, bool isOrdered)
        {
            ParentsNumber = parentsNumber;
            ChildrenNumber = offSpringNumber;
            MinChromosomeLength = minChromosomeLength;
            IsOrdered = isOrdered;
        }

        public IList<IChromosome> Cross(IList<IChromosome> parents)
        {
            IChromosome parent1 = parents[0];
            IChromosome parent2 = parents[1];
            IChromosome offspring1 = parent1.CreateNew();
            IChromosome offspring2 = parent2.CreateNew();

            //YOUR CODE HERE



            return new List<IChromosome> { offspring1, offspring2 };
            
        }
    }
}