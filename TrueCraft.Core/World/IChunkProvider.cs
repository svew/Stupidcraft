using System;

namespace TrueCraft.Core.World
{
    // TODO: this should be server-side only
    /// <summary>
    /// Provides new chunks to worlds. Generally speaking this is a terrain generator.
    /// </summary>
    public interface IChunkProvider
    {
        IChunk GenerateChunk(GlobalChunkCoordinates coordinates);

        GlobalVoxelCoordinates GetSpawn(IDimension dimension);
    }
}
