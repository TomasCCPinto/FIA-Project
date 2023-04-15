using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Reinsertions;


public class Elitism : ReinsertionBase
{
    protected int eliteSize = 0;
    public Elitism(int eliteSize) : base(false, false)
    {
        this.eliteSize = eliteSize;
    }
    
    protected override IList<IChromosome> PerformSelectChromosomes(IPopulation population, IList<IChromosome> offspring, IList<IChromosome> parents)
    {
        // YOUR CODE HERE
        var old_population = population.CurrentGeneration.Chromosomes.OrderByDescending(p => p.Fitness).ToList(); //previous population sorted by fitness

        

        return offspring;
    }
    
}
