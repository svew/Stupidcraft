using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrueCraft.Core.World
{
    /// <summary>
    /// Used to decorate chunks with "decorations" such as trees, flowers, ores, etc.
    /// </summary>
    public interface IChunkDecorator
    {
        void Decorate(IDimension dimension, IChunk chunk, IBiomeRepository biomes);
    }
}
