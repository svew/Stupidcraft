using System;
using TrueCraft.Core.World;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Entities;

namespace TrueCraft.Core.Logic
{
    public interface IItemProvider
    {
        short ID { get; }
        sbyte MaximumStack { get; }
        string DisplayName { get; }
        void ItemUsedOnNothing(ItemStack item, IWorld world, IRemoteClient user);
        void ItemUsedOnEntity(ItemStack item, IEntity usedOn, IWorld world, IRemoteClient user);
        void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IWorld world, IRemoteClient user);
        Tuple<int, int> GetIconTexture(byte metadata);
    }
}
