using System;
using fNbt;
using TrueCraft.Core;
using TrueCraft.Core.World;

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

        [Obsolete("Should be server-side only and only needed during construction.")]
        void UpdateHeightMap();

        // TODO: move to server-side
        [Obsolete("Should be server-side only")]
        bool IsModified { get; }

        /// <summary>
        /// Gets the Biome at the specified column within the Chunk.
        /// </summary>
        /// <param name="x">The x-coordinate of the column relative to the chunk.</param>
        /// <param name="z">The z-coordinate of the column relative to the chunk.</param>
        /// <returns>The Biome.</returns>
        Biome GetBiome(int x, int z);

        DateTime LastAccessed { get; set; }

        // TODO: remove this method
        [Obsolete("Violation of encapsulation")]
        byte[] Data { get; }

        // TODO: move to server-side
        // Documentation of this field at https://minecraft.fandom.com/wiki/Chunk_format?oldid=120992
        // appears to indicate that there are 2 phases to generation of chunks
        // in Beta 1.7.3.  In order to load Beta 1.7.3 worlds, we may need to handle this.
        [Obsolete("Should be server-side only")]
        bool TerrainPopulated { get; set; }

        // TODO: remove this method
        [Obsolete("Violation of encapsulation")]
        NybbleArray Metadata { get; }
        // TODO: remove this method
        [Obsolete("Violation of encapsulation")]
        NybbleArray BlockLight { get; }
        // TODO: remove this method
        [Obsolete("Violation of encapsulation")]
        NybbleArray SkyLight { get; }

        byte GetBlockID(LocalVoxelCoordinates coordinates);
        byte GetMetadata(LocalVoxelCoordinates coordinates);
        byte GetSkyLight(LocalVoxelCoordinates coordinates);
        byte GetBlockLight(LocalVoxelCoordinates coordinates);
        void SetBlockID(LocalVoxelCoordinates coordinates, byte value);
        void SetMetadata(LocalVoxelCoordinates coordinates, byte value);
        void SetSkyLight(LocalVoxelCoordinates coordinates, byte value);
        void SetBlockLight(LocalVoxelCoordinates coordinates, byte value);

        // TODO: move to server-side
        [Obsolete("Should be server-side only")]
        NbtCompound? GetTileEntity(LocalVoxelCoordinates coordinates);

        // TODO: move to server-side
        [Obsolete("Should be server-side only")]
        void SetTileEntity(LocalVoxelCoordinates coordinates, NbtCompound? value);
    }
}
