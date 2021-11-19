using System;
using TrueCraft.Core;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public interface IBlockPhysicsProvider
    {
        BoundingBox? GetBoundingBox(IWorld world, GlobalVoxelCoordinates coordinates);
    }
}
