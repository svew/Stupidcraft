using System;
using TrueCraft.Core.Logic;
using fNbt;
using System.Collections.Generic;

namespace TrueCraft.Core.World
{
    // TODO: Entities
    /// <summary>
    /// An in-game Dimension (eg. OverWorld, The Nether) composed of chunks and blocks.
    /// </summary>
    public interface IDimension : IEnumerable<IChunk>
    {
        /// <summary>
        /// Gets the ID of this Dimension
        /// </summary>
        DimensionID ID { get; }

        string Name { get; set; }
        IBlockRepository BlockRepository { get; set; }

        IChunkProvider ChunkProvider { get; set; }

        long Time { get; set; }

        event EventHandler<BlockChangeEventArgs> BlockChanged;
        event EventHandler<ChunkLoadedEventArgs> ChunkGenerated;
        event EventHandler<ChunkLoadedEventArgs> ChunkLoaded;

        IChunk GetChunk(GlobalChunkCoordinates coordinates, bool generate = true);
        IChunk FindChunk(GlobalVoxelCoordinates coordinates, bool generate = true);

        BlockDescriptor GetBlockData(GlobalVoxelCoordinates coordinates);
        void SetBlockData(GlobalVoxelCoordinates coordinates, BlockDescriptor block);

        byte GetBlockID(GlobalVoxelCoordinates coordinates);
        void SetBlockID(GlobalVoxelCoordinates coordinates, byte value);

        byte GetMetadata(GlobalVoxelCoordinates coordinates);
        void SetMetadata(GlobalVoxelCoordinates coordinates, byte value);

        byte GetBlockLight(GlobalVoxelCoordinates coordinates);
        void SetBlockLight(GlobalVoxelCoordinates coordinates, byte value);

        byte GetSkyLight(GlobalVoxelCoordinates coordinates);
        void SetSkyLight(GlobalVoxelCoordinates coordinates, byte value);

        LocalVoxelCoordinates FindBlockPosition(GlobalVoxelCoordinates coordinates, out IChunk chunk, bool generate = true);

        NbtCompound GetTileEntity(GlobalVoxelCoordinates coordinates);
        void SetTileEntity(GlobalVoxelCoordinates coordinates, NbtCompound value);

        bool IsValidPosition(GlobalVoxelCoordinates position);

        bool IsChunkLoaded(GlobalVoxelCoordinates coordinates);

        void Save();
    }
}
