using System;
using fNbt;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Server
{
    // TODO: this should be server-side only

    /// <summary>
    /// An enum to specify the level of effort to put into finding a Chunk
    /// </summary>
    public enum LoadEffort
    {
        /// <summary>
        /// The Chunk will only be returned if it is already present in memory.
        /// It will not be loaded from disk or generated.
        /// </summary>
        InMemory,

        /// <summary>
        /// The Chunk will be returned if it is in memory.  If it is not in
        /// memory, it will be loaded from disk.  If it is not on disk, it will
        /// not be generated.
        /// </summary>
        Load,

        /// <summary>
        /// The Chunk will always be returned.  If it is not in memory, it will
        /// be loaded from disk.  If it is not on disk, it will be generated.
        /// </summary>
        Generate
    }

    /// <summary>
    /// Contains Dimension methods that are server-side only.
    /// </summary>
    public interface IDimensionServer : IDimension
    {
        event EventHandler<BlockChangeEventArgs> BlockChanged;
        event EventHandler<ChunkLoadedEventArgs> ChunkGenerated;
        event EventHandler<ChunkLoadedEventArgs> ChunkLoaded;

        /// <summary>
        /// Gets the Tile Entity at the given Coordinates.
        /// </summary>
        /// <param name="coordinates">The location of the Tile Entity.</param>
        /// <returns>The Tile Entity or null.  Null will be returned if there is no Tile Entity
        /// at the given coordinates or if the containing Chunk is not loaded into memory.</returns>
        NbtCompound? GetTileEntity(GlobalVoxelCoordinates coordinates);

        void SetTileEntity(GlobalVoxelCoordinates coordinates, NbtCompound? value);

        void Save();

        /// <summary>
        /// Gets the Chunk at the given Coordinates.
        /// </summary>
        /// <param name="coordinates">Specifies which Chunk to return</param>
        /// <param name="loadEffort">Specifies the amount of effort for returning the Chunk</param>
        /// <returns>The requested Chunk or null if it is not present at the specified LoadEffort level.</returns>
        IChunk? GetChunk(GlobalChunkCoordinates coordinates, LoadEffort loadEffort);

        /// <summary>
        /// Gets the Block ID of the Block at the given Coordinates.
        /// </summary>
        /// <param name="coordinates">Specifies the location of the Block</param>
        /// <param name="loadEffort">The amount of effort for locating the Block.</param>
        /// <returns>The Block ID.  If the Block is not present at the specified LoadEffort, the Air Block ID is returned.</returns>
        byte GetBlockID(GlobalVoxelCoordinates coordinates, LoadEffort loadEffort);

        /// <summary>
        /// Gets the metadata for the Block at the specified Coordinates.
        /// </summary>
        /// <param name="coordinates">The Coordinates from which to fetch Metadata</param>
        /// <param name="loadEffort">The amount of effort for locating the Block.</param>
        /// <returns>The Metadata of the Block at the given Coordinates, or zero if
        /// the containing Chunk is not present at the specified LoadEffort.</returns>
        byte GetMetadata(GlobalVoxelCoordinates coordinates, LoadEffort loadEffort);

        /// <summary>
        /// Gets the Block Light Level at the given Coordinates.
        /// </summary>
        /// <param name="coordinates">The Coordinates from which to fetch the Block Light Level.</param>
        /// <param name="loadEffort">The amount of effort for locating the Block.</param>
        /// <returns>The level of Block Light at the given coordinates.</returns>
        /// <remarks>If the Chunk is not available at the specified LoadEffort, zero will be returned.</remarks>
        byte GetBlockLight(GlobalVoxelCoordinates coordinates, LoadEffort loadEffort);

        /// <summary>
        /// Gets the Sky Light Level at the given Coordinates.
        /// </summary>
        /// <param name="coordinates">The Coordinates from which to fetch the Sky Light Level</param>
        /// <param name="loadEffort">The amount of effort for locating the Block.</param>
        /// <returns>The Sky Light Level at the given Coordinates.</returns>
        /// <remarks>If the Chunk is not available at the specified LoadEffort, zero will be returned.</remarks>
        byte GetSkyLight(GlobalVoxelCoordinates coordinates, LoadEffort loadEffort);

        /// <summary>
        /// Gets the full name of the type of Chunk Provider for this Dimension.
        /// </summary>
        string ChunkProvider { get; }
    }
}
