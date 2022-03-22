using System;
using fNbt;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Server
{
    // TODO: this should be server-side only

    /// <summary>
    /// Contains Dimension methods that are server-side only.
    /// </summary>
    public interface IDimensionServer : IDimension
    {
        event EventHandler<BlockChangeEventArgs> BlockChanged;
        event EventHandler<ChunkLoadedEventArgs> ChunkGenerated;
        event EventHandler<ChunkLoadedEventArgs> ChunkLoaded;

        NbtCompound? GetTileEntity(GlobalVoxelCoordinates coordinates);
        void SetTileEntity(GlobalVoxelCoordinates coordinates, NbtCompound? value);

        void Save();
    }
}
