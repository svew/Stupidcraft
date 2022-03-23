using System;
using TrueCraft.Core;
using TrueCraft.Core.World;

namespace TrueCraft.TerrainGen.Biomes
{
    public class TaigaBiome : BiomeProvider
    {
        public override byte ID
        {
            get { return (byte)Biome.Taiga; }
        }

        public override double Temperature
        {
            get { return 0.0f; }
        }

        public override double Rainfall
        {
            get { return 0.0f; }
        }

        public override TreeSpecies[] Trees
        {
            get
            {
                return new[] { TreeSpecies.Spruce };
            }
        }

        public override double TreeDensity
        {
            get
            {
                return 5;
            }
        }
    }
}
