using System;
using System.Collections.Generic;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.World
{
    /// <summary>
    /// A World is a "collection" of Dimensions.
    /// </summary>
    public interface IWorld : IEnumerable<IDimensionServer>
    {
        /// <summary>
        /// Gets the Seed used to generate the World
        /// </summary>
        int Seed { get; }

        /// <summary>
        /// Gets the Name of the World
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the Spawn Point of this world.
        /// </summary>
        PanDimensionalVoxelCoordinates SpawnPoint { get; }

        /// <summary>
        /// Saves the World
        /// </summary>
        void Save();

        /// <summary>
        /// Gets the number of Dimensions in this World.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets a reference to the specified Dimension
        /// </summary>
        /// <param name="index">The ID of the Dimension to get</param>
        /// <returns>A reference to the specified Dimension</returns>
        IDimension this[DimensionID index] { get; }
    }
}
