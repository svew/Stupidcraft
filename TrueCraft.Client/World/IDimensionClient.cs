using System;
using TrueCraft.Core.World;

namespace TrueCraft.Client.World
{
    public interface IDimensionClient : IDimension
    {
        /// <summary>
        /// Adds the given Chunk to the client's current Dimension.
        /// </summary>
        /// <param name="chunk">The Chunk to add.</param>
        void AddChunk(IChunk chunk);
    }
}
