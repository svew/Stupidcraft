using System;
using System.Collections.Concurrent;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Lighting
{
    public class LightingQueue : ILightingQueue
    {
        private readonly ConcurrentStack<LightingOperation> _skyLightingQueue;
        private readonly ConcurrentStack<LightingOperation> _blockLightingQueue;

        public LightingQueue()
        {
            _skyLightingQueue = new ConcurrentStack<LightingOperation>();
            _blockLightingQueue = new ConcurrentStack<LightingOperation>();
        }

        /// <inheritdoc />
        public void Enqueue(BoundingBox box, bool skyLight)
        {
            LightingOperation op = new LightingOperation(box, skyLight);
            if (skyLight)
                _skyLightingQueue.Push(op);
            else
                _blockLightingQueue.Push(op);
        }

        /// <inheritdoc />
        public LightingOperation? Dequeue()
        {
            LightingOperation rv;

            if (_skyLightingQueue.TryPop(out rv))
                return rv;

            if (_blockLightingQueue.TryPop(out rv))
                return rv;

            return null;
        }
    }
}
