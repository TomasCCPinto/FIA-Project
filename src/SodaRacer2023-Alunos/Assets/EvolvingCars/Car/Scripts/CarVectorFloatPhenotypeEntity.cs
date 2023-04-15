using GeneticSharp.Runner.UnityApp.Commons;
using UnityEngine;

namespace GeneticSharp.Runner.UnityApp.Car
{
    public class CarVectorFloatPhenotypeEntity : FloatPhenotypeEntityBase
    {
        public const int VectorSizeBits = 1;
        public const int VectorAngleBits = 1;
        public const int WheelIndexBits = 1;
        public const int WheelRadiusBits = 1;
        public const int PhenotypeSize = VectorSizeBits + VectorAngleBits + WheelIndexBits + WheelRadiusBits;


        public CarVectorFloatPhenotypeEntity(CarSampleConfig config, int entityIndex)
        {
            Phenotypes = new FloatPhenotype[]
            {
                new FloatPhenotype("VectorSize", VectorSizeBits)
                {
                    MinValue = 1,
                    MaxValue = config.MaxVectorSize
                },
                new FloatPhenotype("VectorAngle", VectorAngleBits)
                {
                    MinValue = 0,
                    MaxValue = 359
                },
                new CarWheelIndexPhenotype(config, entityIndex),
                new CarWheelRadiusPhenotype(config, entityIndex)
            };
        }


        public Vector2 Vector
        {
            get
            {
                var size = (float)Phenotypes[0].Value;
                var angle = (float)Phenotypes[1].Value;
                var offset = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * size;

                return Vector2.zero + offset;
            }
        }

        public int WheelIndex
        {
            get
            {
                return (int)Phenotypes[2].Value;
            }
        }

        public float WheelRadius
        {
            get
            {
                var v = (float)Phenotypes[3].Value;
                return v >= 1 ? v : 0;
            }
        }



        class CarWheelIndexPhenotype : FloatPhenotype
        {
            public CarWheelIndexPhenotype(CarSampleConfig config, int entityIndex)
                : base("WheelIndex", WheelIndexBits)
            {
                MinValue = 0;

                // If entityIndex is greater than wheels count, then should not
                // generate any wheel.
                MaxValue = entityIndex < config.WheelsCount
                                               ? config.VectorsCount - 1
                                               : 0;
            }
        }

        class CarWheelRadiusPhenotype : FloatPhenotype
        {
            public CarWheelRadiusPhenotype(CarSampleConfig config, int entityIndex)
                : base("WheelRadius", WheelRadiusBits)
            {
                MinValue = 0;

                // If entityIndex is greater than wheels count, then should not
                // generate any wheel.
                MaxValue = entityIndex < config.WheelsCount
                                               ? MaxValue = config.MaxWheelRadius
                                               : 0;
            }
        }
    }
}