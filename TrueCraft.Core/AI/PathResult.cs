using System;
using System.Collections.Generic;
using TrueCraft.Core.World;

namespace TrueCraft.Core
{
    public class PathResult
    {
        public PathResult()
        {
            Index = 0;
        }

        public IList<GlobalVoxelCoordinates> Waypoints;
        public int Index;
    }
}
