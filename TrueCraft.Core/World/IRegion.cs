using System;
using System.Collections.Generic;

namespace TrueCraft.Core.World
{
    public interface IRegion : IDisposable
    {
        /// <summary>
        /// The location of this IRegion within the world.
        /// </summary>
        RegionCoordinates Position { get; }

        IEnumerable<IChunk> Chunks { get; }

        IChunk GetChunk(LocalChunkCoordinates position, bool generate = true);

        bool IsChunkLoaded(LocalChunkCoordinates position);

        /// <summary>
        /// Marks the chunk for saving in the next Save().
        /// </summary>
        void DamageChunk(LocalChunkCoordinates position);

        void UnloadChunk(LocalChunkCoordinates position);

        void Save(string path);
    }
}
