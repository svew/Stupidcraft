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

        IChunk GenerateChunk(IDimension world, GlobalChunkCoordinates coordinates);

        GlobalVoxelCoordinates GetSpawn(IDimension world);
        void Initialize(IDimension world);
    }
}
