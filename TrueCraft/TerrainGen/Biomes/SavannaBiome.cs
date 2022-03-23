using System;
using TrueCraft.Core;
using TrueCraft.Core.World;

namespace TrueCraft.TerrainGen.Biomes
{
    public class SavannaBiome : BiomeProvider
    {
        public override byte ID
        {
            get { return (byte)Biome.Savanna; }
        }

        public override double Temperature
        {
            get { return 1.2f; }
        }

        public override double Rainfall
        {
            get { return 0.0f; }
        }

        public override TreeSpecies[] Trees
        {
            get
            {
                return new[] { TreeSpecies.Oak };
            }
        }

        public override PlantSpecies[] Plants
        {
            get
            {
                return new[] { PlantSpecies.Deadbush, PlantSpecies.Fern };
            }
        }

        public override double TreeDensity
        {
            get
            {
                return 50;
            }
        }
    }
}
