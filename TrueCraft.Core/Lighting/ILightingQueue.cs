using System;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Lighting
{
    public interface ILightingQueue
    {
        /// <summary>
        /// Creates a Lighting Operation and adds it to the Queue.
        /// </summary>
        /// <param name="seed">The coordinates of the light source that was placed/broken.</param>
        /// <param name="mode">Specifies whether the light source was added or removed.</param>
        /// <param name="kind">Specifies the type of lighting operation to enqueue.</param>
        /// <param name="lightLevel">The level of the light source that was added/removed.</param>
        /// <remarks>If the kind is LightingOperationKind.Initial, the mode and lightLevel do not matter.</remarks>
        void Enqueue(GlobalVoxelCoordinates seed, LightingOperationMode mode,
            LightingOperationKind kind, byte lightLevel);

        /// <summary>
        /// Dequeues a Lighting Operation from the Lighting Queue.
        /// </summary>
        /// <returns>The next Lighting Operation to work on.  Null will
        /// be returned if there are no Lighting Operations queued.</returns>
        LightingOperation? Dequeue();
    }
}
