using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrueCraft.Core.Logic;

namespace TrueCraft.Core.World
{
    // TODO: this interface should be server-side only.
    /// <summary>
    /// Used to decorate chunks with "decorations" such as trees, flowers, ores, etc.
    /// </summary>
    public interface IChunkDecorator
    {
        void Decorate(int seed, IChunk chunk, IBlockRepository blockRepository, IBiomeRepository biomes);
    }
}
