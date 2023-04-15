using System;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Runner.UnityApp.Commons;

namespace GeneticSharp.Runner.UnityApp.Car
{
    [Serializable]
    public class CarBinaryChromosome : BitStringChromosome<CarVectorPhenotypeEntity>
    {
        private CarSampleConfig m_config;
        public CarBinaryChromosome(CarSampleConfig config)
        {
            m_config = config;

            var phenotypeEntities = new CarVectorPhenotypeEntity[config.VectorsCount];

            for (int i = 0; i < phenotypeEntities.Length; i ++)
            {
                phenotypeEntities[i] = new CarVectorPhenotypeEntity(config, i);
            }

            SetPhenotypes(phenotypeEntities);
            CreateGenes();
        }

        public string ID { get; } = System.Guid.NewGuid().ToString();

        public bool Evaluated { get; set; }
        public float MaxDistance { get; set; }
        public float MaxDistanceTime { get; set; }
        new public float Fitness { get; set; }
        public float NumberOfWheels { get; set; }
        public float CarMass { get; set; }
        public bool IsRoadComplete { get; set; } = false;

        public float MaxVelocity 
        { 
            get 
            {
                return MaxDistanceTime > 0 ? MaxDistance / MaxDistanceTime : 0; 
                            
            } 
        }
      
        public override IChromosome CreateNew()
        {
            return new CarBinaryChromosome(m_config);
        }
    }
}