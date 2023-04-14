using GeneticSharp.Domain.Chromosomes;
using System;
using System.Linq;
using GeneticSharp.Infrastructure.Framework.Commons;
using GeneticSharp.Domain.Randomizations;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace GeneticSharp.Runner.UnityApp.Commons
{
    [Serializable]
    public abstract class FloatingPointChromosome<TFPhenotypeEntity> : ChromosomeBase
        where TFPhenotypeEntity : FPhenotypeEntity
    {
        private TFPhenotypeEntity[] m_phenotypeEntities;
        private double[] originalValues;

        protected FloatingPointChromosome()
            : base(2)
        {
        }

        protected void SetPhenotypes(params TFPhenotypeEntity[] phenotypeEntities)
        {
            if (phenotypeEntities.Length == 0)
            {
                throw new ArgumentException("At least one phenotype entity should be informed.", nameof(phenotypeEntities));
            }

            m_phenotypeEntities = phenotypeEntities;
            Resize(m_phenotypeEntities.Sum(e => e.Phenotypes.Sum(p => p.Length)));
        }

        public virtual TFPhenotypeEntity[] GetPhenotypes()
        {
            var genes = GetGenes();
            var skip = 0;
            var entityLength = 0;

            foreach (var entity in m_phenotypeEntities)
            {
                entityLength = entity.GetTotalFloats();
                entity.Load(genes.Skip(skip).Take(entityLength).Select(g => (double)g.Value));
                skip += entityLength;
            }

            return m_phenotypeEntities;
        }

        protected override void CreateGenes()
        {
            var valuesLength = m_phenotypeEntities.Sum(p => p.Phenotypes.Length);
            originalValues = new double[valuesLength];
            FPhenotype phenotype;

            int valueIndex = 0;
            foreach (var entity in m_phenotypeEntities)
            {
                for (int i = 0; i < entity.Phenotypes.Length; i++)
                {
                    phenotype = entity.Phenotypes[i];
                    originalValues[valueIndex] = phenotype.RandomValue();
                    valueIndex++;
                }
            }

            base.CreateGenes();
        }

        public override Gene GenerateGene(int geneIndex)
        {
            return new Gene(originalValues[geneIndex]);
        }
        
        public override string ToString()
        {
            
            return String.Join(";", GetGenes().Select(g => g.Value.ToString()).ToArray());
        }
    }

    public interface FPhenotype
    {
        string Name { get; }
        int Length { get; }
        double MinValue { get; }
        double MaxValue { get; }
        double Value { get; set; }

        double RandomValue();
    }

    public interface FPhenotypeEntity
    {
        FPhenotype[] Phenotypes { get; }
        void Load(IEnumerable<double> entityGenes);
    }

    public static class FloatPhenotypeEntityExtensions
    {
        public static int GetTotalFloats(this FPhenotypeEntity entity)
        {
            return entity.Phenotypes.Sum(p => p.Length);
        }
    }

    public abstract class FloatPhenotypeEntityBase : FPhenotypeEntity
    {
        public FPhenotype[] Phenotypes { get; protected set; }

        public void Load(IEnumerable<double> entityGenes)
        {
            var skip = 0;
           
            foreach(var p in Phenotypes)
            {
                p.Value = GetValue(entityGenes, skip, p);
                skip += p.Length;
            }
        }
      
        private double GetValue(IEnumerable<double> genes, int skip, FPhenotype phenotype)
        {
            var value = genes.Skip(skip).Take(phenotype.Length).FirstOrDefault();
            
            if (value < phenotype.MinValue)
                return phenotype.MinValue;

            if (value > phenotype.MaxValue)
                return phenotype.MaxValue;

            return value;
        }
    }

    [DebuggerDisplay("{Name} = {MinValue} <= {Value} <= {MaxValue}")]
    public class FloatPhenotype : FPhenotype
    {
        public FloatPhenotype(string name, int length)
        {
            Name = name;
            Length = length;
        }

        public string Name { get; }
        public int Length { get; }
        public double MinValue { get; set; } = 0;
        public double MaxValue { get; set; } = 100;
        public virtual double Value { get; set; }
    
        public virtual double RandomValue()
        {
            return RandomizationProvider.Current.GetDouble(MinValue, MaxValue + 1);
        }


        
    }

    


}