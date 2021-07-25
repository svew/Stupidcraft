using System;
using System.Collections.Generic;
using TrueCraft.API.World;

namespace TrueCraft.API
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