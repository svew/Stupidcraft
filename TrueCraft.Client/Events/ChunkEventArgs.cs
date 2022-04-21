using System;
using TrueCraft.Core.World;

namespace TrueCraft.Client.Events
{
    public class ChunkEventArgs : EventArgs
    {
        public IChunk Chunk { get; }

        public ChunkEventArgs(IChunk chunk)
        {
            Chunk = chunk;
        }
    }
}