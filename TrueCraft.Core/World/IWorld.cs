using System;
using TrueCraft.Core.Logic;
using fNbt;
using System.Collections.Generic;

namespace TrueCraft.Core.World
{
    // TODO: Entities
    /// <summary>
    /// An in-game world composed of chunks and blocks.
    /// </summary>
    public interface IWorld : IEnumerable<IChunk>
    {
        string Name { get; set; }
        IBlockRepository BlockRepository { get; set; }
        int Seed { get; set; }
        IBiomeMap BiomeDiagram { get; set; }
        IChunkProvider ChunkProvider { get; set; }

        GlobalVoxelCoordinates SpawnPoint { get; set; }

        long Time { get; set; }

        event EventHandler<BlockChangeEventArgs> BlockChanged;
        event EventHandler<ChunkLoadedEventArgs> ChunkGenerated;
        event EventHandler<ChunkLoadedEventArgs> ChunkLoaded;

        IChunk GetChunk(GlobalChunkCoordinates coordinates, bool generate = true);
        IChunk FindChunk(GlobalVoxelCoordinates coordinates, bool generate = true);
        byte GetBlockID(GlobalVoxelCoordinates coordinates);
        byte GetMetadata(GlobalVoxelCoordinates coordinates);
        byte GetBlockLight(GlobalVoxelCoordinates coordinates);
        byte GetSkyLight(GlobalVoxelCoordinates coordinates);
        LocalVoxelCoordinates FindBlockPosition(GlobalVoxelCoordinates coordinates, out IChunk chunk, bool generate = true);
        NbtCompound GetTileEntity(GlobalVoxelCoordinates coordinates);
        BlockDescriptor GetBlockData(GlobalVoxelCoordinates coordinates);
        void SetBlockData(GlobalVoxelCoordinates coordinates, BlockDescriptor block);
        void SetBlockID(GlobalVoxelCoordinates coordinates, byte value);
        void SetMetadata(GlobalVoxelCoordinates coordinates, byte value);
        void SetSkyLight(GlobalVoxelCoordinates coordinates, byte value);
        void SetBlockLight(GlobalVoxelCoordinates coordinates, byte value);
        void SetTileEntity(GlobalVoxelCoordinates coordinates, NbtCompound value);
        bool IsValidPosition(GlobalVoxelCoordinates position);
        bool IsChunkLoaded(GlobalVoxelCoordinates coordinates);
        void Save();
        void Save(string path);
    }
}
