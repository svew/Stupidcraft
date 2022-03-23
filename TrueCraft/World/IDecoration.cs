using System;
using TrueCraft.Core.World;

namespace TrueCraft.World
{
    // TODO: this interface should be moved to server-side only
    public interface IDecoration
    {
        bool ValidLocation(LocalVoxelCoordinates location);
        bool GenerateAt(int seed, IChunk chunk, LocalVoxelCoordinates location);
    }
}
