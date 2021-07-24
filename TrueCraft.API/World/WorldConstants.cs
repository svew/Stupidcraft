using System;
namespace TrueCraft.API.World
{
    public static class WorldConstants
    {
        /// <summary>
        /// The width (X-direction) of a Chunk in Blocks.
        /// </summary>
        public const int ChunkWidth = 16;

        /// <summary>
        /// The depth (Z-direction) of a Chunk in Blocks.
        /// </summary>
        public const int ChunkDepth = 16;

        /// <summary>
        /// The Height (Y-direction) of the World in Blocks.
        /// </summary>
        public const int Height = 128;

        /// <summary>
        /// The width (X-direction) of a Region in Chunks.
        /// </summary>
        public const int RegionWidth = 32;

        /// <summary>
        /// The depth (Z-Direction) of a Region in Chunks.
        /// </summary>
        public const int RegionDepth = 32;
    }
}
