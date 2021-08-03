using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrueCraft.API.World
{
    public class BiomeCell
    {
        public byte BiomeID { get; }
        public GlobalColumnCoordinates CellPoint { get; }

        public BiomeCell(byte biomeID, GlobalColumnCoordinates cellPoint)
        {
            this.BiomeID = biomeID;
            this.CellPoint = cellPoint;
        }
    }

    public interface IBiomeMap
    {
        IList<BiomeCell> BiomeCells { get; }
        void AddCell(BiomeCell cell);
        byte GetBiome(GlobalColumnCoordinates location);
        byte GenerateBiome(int seed, IBiomeRepository biomes, GlobalColumnCoordinates location, bool spawn);
        BiomeCell ClosestCell(GlobalColumnCoordinates location);
        double ClosestCellPoint(GlobalColumnCoordinates location);
    }
}