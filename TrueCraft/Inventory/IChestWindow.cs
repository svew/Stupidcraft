using System;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Inventory
{
    public interface IChestWindow : TrueCraft.Core.Inventory.IChestWindow<IServerSlot>
    {
        IDimension Dimension { get; }

        GlobalVoxelCoordinates Location { get; }

        GlobalVoxelCoordinates? OtherHalf { get; }
    }
}
