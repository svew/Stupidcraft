using System;
using System.Collections.Generic;
using TrueCraft.Core.World;

namespace TrueCraft.Core
{
    public class EmptyGenerator : IChunkProvider
    {
        public IChunk GenerateChunk(IWorld world, GlobalChunkCoordinates coordinates)
        {
            return new Chunk(coordinates);
        }

        public GlobalVoxelCoordinates GetSpawn(IWorld world)
        {
            return GlobalVoxelCoordinates.Zero;
        }

        public void Initialize(IWorld world)
        {
        }

        public IList<IChunkDecorator> ChunkDecorators { get; set; }
    }
}