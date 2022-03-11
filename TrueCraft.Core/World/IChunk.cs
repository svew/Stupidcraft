using System;
using fNbt;
using System.Collections.Generic;

namespace TrueCraft.Core.World
{
    public interface IChunk : IEventSubject, IDisposable
    {
        /// <summary>
        /// Gets the distance at which this Chunk is located away
        /// from the origin (in Chunks) in the X-direction.
        /// </summary>
        int X { get; }

        /// <summary>
        /// Gets the distance at which this Chunk is located away
        /// from the origin (in Chunks) in the Z-direction.
        /// </summary>
        int Z { get; }

        /// <summary>
        /// Gets the coordinates (In chunks) of the location of this Chunk.
        /// </summary>
        GlobalChunkCoordinates Coordinates { get; }

        /// <summary>
        /// Gets the y-coordinate of the highest block in this Chunk
        /// </summary>
        int MaxHeight { get; }

        /// <summary>
        /// Gets the y-coordinate of the highest block in the specified Column of Blocks
        /// </summary>
        /// <param name="x">The x-coordinate of the column relative to the chunk.</param>
        /// <param name="z">The z-coordinate of the column relative to the chunk.</param>
        /// <returns>The y-coordinate of the highest block</returns>
        int GetHeight(int x, int z);
        void UpdateHeightMap();

        bool IsModified { get; set; }
        bool LightPopulated { get; set; }

        byte[] Biomes { get; }
        DateTime LastAccessed { get; set; }
        byte[] Data { get; }
        bool TerrainPopulated { get; set; }

        Dictionary<LocalVoxelCoordinates, NbtCompound> TileEntities { get; }

        NibbleSlice Metadata { get; }
        NibbleSlice BlockLight { get; }
        NibbleSlice SkyLight { get; }
        IRegion ParentRegion { get; set; }

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
