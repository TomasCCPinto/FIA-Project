using GeneticSharp.Domain.Terminations;
using GeneticSharp.Domain;
using UnityEngine;

namespace GeneticSharp.Runner.UnityApp.Car
{
    public class GenerationsTermination : TerminationBase
    {
        protected int maxGenerations = 0;

        public GenerationsTermination(int maxGenerations) 
        {
            this.maxGenerations = maxGenerations;
        }


    
		protected override bool PerformHasReached(IGeneticAlgorithm geneticAlgorithm)
		{
            var ga = geneticAlgorithm as GeneticAlgorithm;

            foreach (var c in ga.Population.CurrentGeneration.Chromosomes)
            {
                c.Fitness = null;
            }

            return ga.Population.CurrentGeneration.Number >= this.maxGenerations;
		}
	}
}