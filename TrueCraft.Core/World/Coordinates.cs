using System;
using TrueCraft.API;

namespace TrueCraft.Core.World
{
    /// <summary>
    /// This class provides static methods for converting between the various coordinate systems.
    /// </summary>
    /// <remarks>
    /// <para>Block Coordinates count the number of blocks away from the origin.
    /// These are the coordinates, which will be most familiar to Players.
    /// </para>
    /// <para>Global Chunk Coordinates count the number of chunks away from the origin.
    /// These are 2-dimensional (X & Z).  They are the Block Coordinates divided by
    /// the number of blocks per Chunk.
    /// </para>
    /// <para>
    /// Local Chunk Coordinates count the number of Chunks within a Region starting from
    /// the north-west corner.
    /// </para>
    /// <para>
    /// Region Coordinates count the number of Regions away from the origin.
    /// </para>
    /// </remarks>
    public static class Coordinates
    {
        /// <summary>
        /// Converts a Global Chunk Coordinate to Local Chunk Coordinate.
        /// </summary>
        /// <param name="global">The Global Chunk Coordinate to convert.</param>
        /// <returns>The Local Chunk Coordinates.</returns>
        public static Coordinates2D GlobalChunkToLocalChunk(Coordinates2D global)
        {
            int localX;
            int localZ;

            if (global.X >= 0)
                localX = global.X / Region.Width;
            else
                localX = (global.X + 1) / Region.Width - 1;

            if (global.Z >= 0)
                localZ = global.Z / Region.Depth;
            else
                localZ = (global.Z + 1) / Region.Depth - 1;

            return new Coordinates2D(global.X - localX * Region.Width, global.Z - localZ * Region.Depth);
        }

        /// <summary>
        /// Converts a Global Chunk Coordinate to a Region Coordinate.
        /// </summary>
        /// <param name="global">The Global Chunk Coordinate to convert.</param>
        /// <returns>The Region Coordinates of the given Chunk Coordinate's containing Region.</returns>
        public static Coordinates2D GlobalChunkToRegion(Coordinates2D global)
        {
            int regionX;
            int regionZ;

            if (global.X >= 0)
                regionX = global.X / Region.Width;
            else
                regionX = (global.X + 1) / Region.Width - 1;

            if (global.Z >= 0)
                regionZ = global.Z / Region.Depth;
            else
                regionZ = (global.Z + 1) / Region.Width - 1;

            return new Coordinates2D(regionX, regionZ);
        }

        /// <summary>
        /// Converts Block Coordinates to the Global Chunk Coordinates of the
        /// Chunk containing the given Block Coordinates.
        /// </summary>
        /// <param name="block">The Block Coordinates to convert.</param>
        /// <returns>The Global Chunk Coordinates of the containing Chunk.</returns>
        public static Coordinates2D BlockToGlobalChunk(Coordinates2D block)
        {
            int chunkX;
            int chunkZ;

            if (block.X >= 0)
                chunkX = block.X / Chunk.Width;
            else
                chunkX = (block.X + 1) / Chunk.Width - 1;

            if (block.Z >= 0)
                chunkZ = (block.Z / Chunk.Depth);
            else
                chunkZ = (block.Z + 1) / Chunk.Depth - 1;

            return new Coordinates2D(chunkX, chunkZ);
        }

        /// <summary>
        /// Converts Block Coordinates to the Global Chunk Coordinates of the
        /// Chunk containing the given Block Coordinates.
        /// </summary>
        /// <param name="block">The Block Coordinates to convert.</param>
        /// <returns>The Global Chunk Coordinates of the containing Chunk.</returns>
        public static Coordinates2D BlockToGlobalChunk(Vector3 block)
        {
            int x = (int)Math.Floor(block.X);
            int z = (int)Math.Floor(block.Z);

            int chunkX;
            int chunkZ;

            if (x >= 0)
                chunkX = x / Chunk.Width;
            else
                chunkX = (x + 1) / Chunk.Width - 1;

            if (z >= 0)
                chunkZ = z / Chunk.Depth;
            else
                chunkZ = (z + 1) / Chunk.Depth - 1;

            return new Coordinates2D(chunkX, chunkZ);
        }

        /// <summary>
        /// Converts Block Coordinates to the coordinates of the containing Region.
        /// </summary>
        /// <param name="block">The Block Coordinates to convert.</param>
        /// <returns>The Region Coordinates in which the Block is contained.</returns>
        public static Coordinates2D BlockToRegion(Coordinates2D block)
        {
            return BlockToRegion(block.X, block.Z);
        }

        /// <summary>
        /// Converts Block Coordinates to the coordinates of the containing Region.
        /// </summary>
        /// <param name="block">The Block Coordinates to convert.</param>
        /// <returns>The Region Coordinates in which the Block is contained.</returns>
        public static Coordinates2D BlockToRegion(Coordinates3D block)
        {
            return BlockToRegion(block.X, block.Z);
        }

        /// <summary>
        /// Converts Block Coordinates to the coordinates of the containing Region.
        /// </summary>
        /// <param name="x">The Block X-Coordinate to convert.</param>
        /// <param name="z">The Block Z-Coordinate to convert.</param>
        /// <returns>The Region Coordinates in which the Block is contained.</returns>
        private static Coordinates2D BlockToRegion(int x, int z)
        {
            int regionX;
            int regionZ;

            if (x >= 0)
                regionX = x / (Chunk.Width * Region.Width);
            else
                regionX = (x + 1) / (Chunk.Width * Region.Width) - 1;

            if (z >= 0)
                regionZ = z / (Chunk.Depth * Region.Depth);
            else
                regionZ = (z + 1) / (Chunk.Depth * Region.Depth) - 1;

            return new Coordinates2D(regionX, regionZ);
        }

        /// <summary>
        /// Converts Block Coordinates to Local Chunk Coordinates.
        /// </summary>
        /// <param name="block">The Block Coordinates to convert.</param>
        /// <returns>The Local Chunk Coordinates of the Chunk containing the given Block Coordinates.</returns>
        public static Coordinates2D BlockToLocalChunk(Coordinates2D block)
        {
            return BlockToLocalChunk(block.X, block.Z);
        }

        /// <summary>
        /// Converts Block Coordinates to Local Chunk Coordinates.
        /// </summary>
        /// <param name="block">The Block Coordinates to convert.</param>
        /// <returns>The Local Chunk Coordinates of the Chunk containing the given Block Coordinates.</returns>
        public static Coordinates2D BlockToLocalChunk(Coordinates3D block)
        {
            return BlockToLocalChunk(block.X, block.Z);
        }

        /// <summary>
        /// Converts Block Coordinates to Local Chunk Coordinates.
        /// </summary>
        /// <param name="x">The Block X-Coordinate to convert.</param>
        /// <param name="z">The Block Z-Coordinate to convert.</param>
        /// <returns>The Local Chunk Coordinates of the Chunk containing the given Block Coordinates.</returns>
        private static Coordinates2D BlockToLocalChunk(int x, int z)
        {
            Coordinates2D region = BlockToRegion(x, z);
            int localX = (x - region.X * Region.Width * Chunk.Width) / Chunk.Width;
            int localZ = (z - region.Z * Region.Depth * Chunk.Depth) / Chunk.Depth;

            return new Coordinates2D(localX, localZ);
        }
    }
}
