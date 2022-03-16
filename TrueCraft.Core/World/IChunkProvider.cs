using System;
using System.Collections.Generic;

namespace TrueCraft.Core.World
{
    /// <summary>
    /// Provides new chunks to worlds. Generally speaking this is a terrain generator.
    /// </summary>
    public interface IChunkProvider
    {
        IList<IChunkDecorator> ChunkDecorators { get; }

        IChunk GenerateChunk(IDimension dimension, GlobalChunkCoordinates coordinates);

        GlobalVoxelCoordinates GetSpawn(IDimension dimension);
        void Initialize(IDimension dimension);
    }
}
