using System;
using System.Collections.Generic;
using TrueCraft.Core.World;

namespace TrueCraft.World
{
    internal interface IRegion : IDisposable
    {
        /// <summary>
        /// The location of this IRegion within the world.
        /// </summary>
        RegionCoordinates Position { get; }

        IEnumerable<IChunk> Chunks { get; }

        /// <summary>
        /// Gets the Chunk at the given location within the Region.
        /// </summary>
        /// <param name="position">The location of the Chunk to return.</param>
        /// <returns>The requested Chunk or null, if it has never been generated.</returns>
        /// <remarks>If the chunk is in memory, it will be returned.  If the Chunk is on disk,
        /// it will be loaded into memory and returned.  If the Chunk is not in memory or on disk,
        /// it has never been generated, and null will be returned.
        /// </remarks>
        IChunk? GetChunk(LocalChunkCoordinates position);

        /// <summary>
        /// Gets the Chunk at the given location within the Region
        /// </summary>
        /// <param name="position">The location of the Chunk to return.</param>
        /// <returns>If the Chunk is in memory, it will be returned.
        /// If the Chunk is not in memory, but is on disk, it will be loaded
        /// into memory and returned.</returns>
        IChunk? LoadChunk(LocalChunkCoordinates position);

        /// <summary>
        /// Sets the chunk at the specified local position to the given value.
        /// </summary>
        /// <param name="chunk">The Chunk to add to this Region.</param>
        void AddChunk(IChunk chunk);

        bool IsChunkLoaded(LocalChunkCoordinates position);

        /// <summary>
        /// Saves the Region to disk.
        /// </summary>
        void Save();

        event EventHandler<ChunkLoadedEventArgs> ChunkLoaded;
    }
}
