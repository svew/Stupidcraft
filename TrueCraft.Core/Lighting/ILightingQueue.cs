using System;

namespace TrueCraft.Core.Lighting
{
    public interface ILightingQueue
    {
        /// <summary>
        /// Creates a Lighting Operation and adds it to the Queue.
        /// </summary>
        /// <param name="box">The Bounding Box within which Lighting will be updated.</param>
        /// <param name="skyLight">True for a SkyLight operation; false for Block Lighting.</param>
        void Enqueue(BoundingBox box, bool skyLight);

        /// <summary>
        /// Dequeues a Lighting Operation from the Lighting Queue.
        /// </summary>
        /// <returns>The next Lighting Operation to work on.  Null will
        /// be returned if there are no Lighting Operations queued.</returns>
        LightingOperation? Dequeue();
    }
}
