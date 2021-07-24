using System;
using fNbt;
using System.Collections.Generic;

namespace TrueCraft.API.World
{
    public interface IChunk : IEventSubject, IDisposable
    {
        int X { get; }
        int Z { get; }
        int MaxHeight { get; }

        GlobalChunkCoordinates Coordinates { get; }

        bool IsModified { get; set; }
        bool LightPopulated { get; set; }
        int[] HeightMap { get; }
        byte[] Biomes { get; }
        DateTime LastAccessed { get; set; }
        byte[] Data { get; }
        bool TerrainPopulated { get; set; }

        Dictionary<LocalVoxelCoordinates, NbtCompound> TileEntities { get; }

        NibbleSlice Metadata { get; }
        NibbleSlice BlockLight { get; }
        NibbleSlice SkyLight { get; }
        IRegion ParentRegion { get; set; }
        int GetHeight(byte x, byte z);
        void UpdateHeightMap();

        byte GetBlockID(LocalVoxelCoordinates coordinates);
        byte GetMetadata(LocalVoxelCoordinates coordinates);
        byte GetSkyLight(LocalVoxelCoordinates coordinates);
        byte GetBlockLight(LocalVoxelCoordinates coordinates);
        void SetBlockID(LocalVoxelCoordinates coordinates, byte value);
        void SetMetadata(LocalVoxelCoordinates coordinates, byte value);
        void SetSkyLight(LocalVoxelCoordinates coordinates, byte value);
        void SetBlockLight(LocalVoxelCoordinates coordinates, byte value);
        NbtCompound GetTileEntity(LocalVoxelCoordinates coordinates);
        void SetTileEntity(LocalVoxelCoordinates coordinates, NbtCompound value);
    }
}