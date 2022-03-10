using System;
using System.Collections.Generic;
using TrueCraft.Core.World;

namespace TrueCraft.Core
{
    public class EmptyGenerator : IChunkProvider
    {
        public IChunk GenerateChunk(IDimension world, GlobalChunkCoordinates coordinates)
        {
            return new Chunk(coordinates);
        }

        public GlobalVoxelCoordinates GetSpawn(IDimension world)
        {
            return GlobalVoxelCoordinates.Zero;
        }

        public void Initialize(IDimension world)
        {
        }

        public IList<IChunkDecorator> ChunkDecorators { get; set; }
    }
}