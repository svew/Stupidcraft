using System;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Lighting
{
    /// <summary>
    /// Specifies whether a light is being added or removed.
    /// </summary>
    public enum LightingOperationMode
    {
        /// <summary>
        /// Specifies that a Light Source has been added.
        /// </summary>
        Add,

        /// <summary>
        /// Specifies that a Light Source has been removed.
        /// </summary>
        Subtract,

        /// <summary>
        /// Specifies that a Block has been placed or removed.
        /// This may cause lighting changes around the Block.
        /// </summary>
        BlockUpdate
    }

    /// <summary>
    /// Specifies the type of Lighting Operation.
    /// </summary>
    public enum LightingOperationKind
    {
        Initial,
        Sky,
        Block
    }

    public class LightingOperation
    {
        private readonly GlobalVoxelCoordinates _seed;
        private readonly LightingOperationMode _mode;
        private readonly LightingOperationKind _kind;
        private readonly byte _lightLevel;

        /// <summary>
        /// Constructs a new Lighting Operation object.
        /// </summary>
        /// <param name="seed">The coordinates of the changed block which initiated this Lighting Operation.</param>
        /// <param name="mode">Specifies whether light is being added or subtracted.</param>
        /// <param name="kind">Specifies the kind of the Lighting Operation</param>
        /// <param name="lightLevel">Specifies the level of light that was added or subtracted.</param>
        public LightingOperation(GlobalVoxelCoordinates seed,
            LightingOperationMode mode, LightingOperationKind kind, byte lightLevel)
        {
            _seed = seed;
            _mode = mode;
            _kind = kind;
            _lightLevel = lightLevel;
        }

        /// <summary>
        /// Gets the location of the change that initiated this Lighting Operation.
        /// </summary>
        public GlobalVoxelCoordinates Seed { get => _seed; }

        /// <summary>
        /// Gets whether light is being added or removed.
        /// </summary>
        public LightingOperationMode Mode { get => _mode; }

        /// <summary>
        /// Gets the type of Lighting being updated.
        /// </summary>
        public LightingOperationKind Kind { get => _kind; }

        /// <summary>
        /// Specifies the Light Level that was added or subtracted at the Seed location.
        /// </summary>
        public byte LightLevel { get => _lightLevel; }
    }
}
