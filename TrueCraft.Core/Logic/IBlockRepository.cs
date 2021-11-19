using System;

namespace TrueCraft.Core.Logic
{
    /// <summary>
    /// Providers block providers for a server.
    /// </summary>
    public interface IBlockRepository
    {
        /// <summary>
        /// Gets this repository's block provider for the specified block ID. This may return null
        /// if the block ID in question has no corresponding block provider.
        /// </summary>
        IBlockProvider GetBlockProvider(byte id);
    }
}
