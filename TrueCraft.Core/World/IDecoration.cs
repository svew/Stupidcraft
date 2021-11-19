using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrueCraft.Core.World
{
    public interface IDecoration
    {
        bool ValidLocation(LocalVoxelCoordinates location);
        bool GenerateAt(IWorld world, IChunk chunk, LocalVoxelCoordinates location);
    }
}
