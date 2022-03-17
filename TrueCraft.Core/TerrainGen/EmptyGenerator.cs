using System;
using System.Collections.Generic;
using TrueCraft.Core.World;

namespace TrueCraft.Core.TerrainGen
{
    public class EmptyGenerator : Generator
    {
        public EmptyGenerator(int seed, IDimension dimension) : base(seed, dimension)
        {

        }

        public override IChunk GenerateChunk(GlobalChunkCoordinates coordinates)
        {
            return new Chunk(coordinates);
        }

        public override GlobalVoxelCoordinates GetSpawn(IDimension dimension)
        {
            return GlobalVoxelCoordinates.Zero;
        }
    }
}