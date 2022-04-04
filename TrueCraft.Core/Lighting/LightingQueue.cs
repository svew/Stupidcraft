using System;
using System.Collections.Concurrent;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Lighting
{
    public class LightingQueue : ILightingQueue
    {
        private readonly ConcurrentStack<LightingOperation> _initialLightingStack;
        private readonly ConcurrentStack<LightingOperation> _skyLightingQueue;
        private readonly ConcurrentStack<LightingOperation> _blockLightingQueue;

        public LightingQueue()
        {
            _initialLightingStack = new ConcurrentStack<LightingOperation>();
            _skyLightingQueue = new ConcurrentStack<LightingOperation>();
            _blockLightingQueue = new ConcurrentStack<LightingOperation>();
        }

        /// <inheritdoc />
        public void Enqueue(GlobalVoxelCoordinates seed, LightingOperationMode mode,
            LightingOperationKind kind, byte lightLevel)
        {
            LightingOperation op = new LightingOperation(seed, mode, kind, lightLevel);

            switch(kind)
            {
                case LightingOperationKind.Initial:
                    _initialLightingStack.Push(op);
                    break;

                case LightingOperationKind.Sky:
                    _skyLightingQueue.Push(op);
                    break;

                case LightingOperationKind.Block:
                    _blockLightingQueue.Push(op);
                    break;
            }
        }

        /// <inheritdoc />
        public LightingOperation? Dequeue()
        {
            LightingOperation rv;

            if (_initialLightingStack.TryPop(out rv))
                return rv;

            if (_skyLightingQueue.TryPop(out rv))
                return rv;

            if (_blockLightingQueue.TryPop(out rv))
                return rv;

            return null;
        }
    }
}
