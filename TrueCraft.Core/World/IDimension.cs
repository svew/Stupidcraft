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
    /// <remarks>
    /// <para>
    /// This interface contains methods which are common to both the Client and the Server.
    /// </para>
    /// <para>
    /// Implementations of this interface's methods and properties must neither load nor
    /// generate Chunks.
    /// </para>
    /// </remarks>
    public interface IDimension : IEnumerable<IChunk>
    {
        /// <summary>
        /// Gets the ID of this Dimension
        /// </summary>
        DimensionID ID { get; }

        /// <summary>
        /// Gets the name of this Dimension
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the BlockRepository in use for this Dimension.
        /// </summary>
        IBlockRepository BlockRepository { get; }

        /// <summary>
        /// Gets or sets the Time of Day (in ticks).
        /// </summary>
        long TimeOfDay { get; set; }

        /// <summary>
        /// Gets the Chunk at the specified Global Chunk Coordinates, if it is loaded.
        /// </summary>
        /// <param name="coordinates">Specifies the location of the Chunk to get.</param>
        /// <returns>The requested Chunk or null if it is not loaded.</returns>
        /// <remarks>Implementations must neither generate nor load the requested Chunk.
        /// It is only to be returned, if already loaded.</remarks>
        IChunk? GetChunk(GlobalChunkCoordinates coordinates);

        /// <summary>
        /// Gets the Chunk containing the given Global Voxel Coordinates, if it is loaded.
        /// </summary>
        /// <param name="coordinates">Specifies the location of the Chunk to get.</param>
        /// <returns>The requested Chunk or null if it is not loaded.</returns>
        /// <remarks>Implementations must neither generate nor load the requested Chunk.
        /// It is only to be returned, if already loaded.</remarks>
        IChunk? GetChunk(GlobalVoxelCoordinates coordinates);

        /// <summary>
        /// Gets data about the Block at the given coordinates.
        /// </summary>
        /// <param name="coordinates">Specifies the coordinates of the block to retrieve.</param>
        /// <returns>Information about the specified Block</returns>
        /// <remarks>If the containing Chunk is not loaded, this will return an air block.</remarks>
        BlockDescriptor GetBlockData(GlobalVoxelCoordinates coordinates);

        // TODO: What if a piston pushes a block into an unloaded chunk?
        /// <summary>
        /// Sets information about the Block at the given coordinates.
        /// </summary>
        /// <param name="coordinates">Specifies the coordinates of the Block to change.</param>
        /// <param name="block">New information about the Block</param>
        /// <remarks>If the Chunk is not loaded, this call will be ignored.</remarks>
        void SetBlockData(GlobalVoxelCoordinates coordinates, BlockDescriptor block);

        /// <summary>
        /// Gets the Block ID of the Block at the specified Coordinates.
        /// </summary>
        /// <param name="coordinates">The coordinates of the Block to fetch.</param>
        /// <returns>The Block ID.  If the Chunk is not loaded, this will return an Air Block ID.</returns>
        byte GetBlockID(GlobalVoxelCoordinates coordinates);

        /// <summary>
        /// Sets the Block ID at the specified Coordinates.
        /// </summary>
        /// <param name="coordinates">The coordinates to update</param>
        /// <param name="value">The Block ID to set at that location.</param>
        /// <remarks>If the Chunk is not loaded, this call is ignored.</remarks>
        void SetBlockID(GlobalVoxelCoordinates coordinates, byte value);

        /// <summary>
        /// Gets the metadata for the Block at the specified Coordinates.
        /// </summary>
        /// <param name="coordinates">The Coordinates from which to fetch Metadata</param>
        /// <returns>The Metadata of the Block at the given Coordinates</returns>
        /// <remarks>If the Chunk is not loaded, zero will be returned.</remarks>
        byte GetMetadata(GlobalVoxelCoordinates coordinates);

        /// <summary>
        /// Sets the metadata for the Block at the specified Coordinates.
        /// </summary>
        /// <param name="coordinates">The Coordinates at which Metadata will be set.</param>
        /// <param name="value">The new value of the Metadata.</param>
        /// <remarks>If the Chunk is not loaded, this call will be ignored.</remarks>
        void SetMetadata(GlobalVoxelCoordinates coordinates, byte value);

        /// <summary>
        /// Gets the Block Light Level at the given Coordinates.
        /// </summary>
        /// <param name="coordinates">The Coordinates from which to fetch the Block Light Level.</param>
        /// <returns>The level of Block Light at the given coordinates.</returns>
        /// <remarks>If the Chunk is not loaded, zero will be returned.</remarks>
        byte GetBlockLight(GlobalVoxelCoordinates coordinates);

        /// <summary>
        /// Sets the Block Light Level at the given Coordinates
        /// </summary>
        /// <param name="coordinates">The Coordinates at which to set the Block Light Level.</param>
        /// <param name="value">The new Block Light Level.</param>
        /// <remarks>If the Chunk is not loaded, this call will be ignored.</remarks>
        void SetBlockLight(GlobalVoxelCoordinates coordinates, byte value);

        /// <summary>
        /// Gets the Sky Light Level at the given Coordinates.
        /// </summary>
        /// <param name="coordinates">The Coordinates from which to fetch the Sky Light Level</param>
        /// <returns>The Sky Light Level at the given Coordinates.</returns>
        /// <remarks>If the Chunk is not loaded, zero will be returned.</remarks>
        byte GetSkyLight(GlobalVoxelCoordinates coordinates);

        /// <summary>
        /// Sets the Sky Light Level at the given Coordinates.
        /// </summary>
        /// <param name="coordinates">The Coordinates at which to set the Sky Light Level.</param>
        /// <param name="value">The new value of Sky Light</param>
        /// <remarks>If the Chunk is not loaded, this call will be ignored.</remarks>
        void SetSkyLight(GlobalVoxelCoordinates coordinates, byte value);

        /// <summary>
        /// A convenience method for fetching a chunk and converting from Global Voxel Coordinates
        /// to Local Voxel Coordinates.
        /// </summary>
        /// <param name="coordinates">The Global Voxel Coordinates of a Block</param>
        /// <param name="chunk">returns the Chunk containing the given Coordinates, or null if the Chunk is not loaded.</param>
        /// <returns>The Local Voxel Coordinates corresponding to the given Global Voxel Coordinates.</returns>
        LocalVoxelCoordinates FindBlockPosition(GlobalVoxelCoordinates coordinates, out IChunk? chunk);

        /// <summary>
        /// Determines if the given Coordinates are valid for this Dimension.
        /// </summary>
        /// <param name="position">The Coordinates to check</param>
        /// <returns>True if the Coordinates are valid; false otherwise.</returns>
        bool IsValidPosition(GlobalVoxelCoordinates position);

        /// <summary>
        /// Determines whether or not the chunk containing the given Coordinates is loaded.
        /// </summary>
        /// <param name="coordinates">The Coordinates too check.</param>
        /// <returns>True if the Chunk is loaded; false otherwise.</returns>
        bool IsChunkLoaded(GlobalVoxelCoordinates coordinates);
    }
}
