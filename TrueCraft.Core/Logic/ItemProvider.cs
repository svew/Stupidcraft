using System;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic
{
    public abstract class ItemProvider : IItemProvider
    {
        public abstract short ID { get; }

        public abstract Tuple<int, int> GetIconTexture(byte metadata);

        public virtual sbyte MaximumStack { get { return 64; } }

        public virtual string DisplayName { get { return string.Empty; } }

        public virtual void ItemUsedOnEntity(ItemStack item, IEntity usedOn, IWorld world, IRemoteClient user)
        {
            // This space intentionally left blank
        }

        public virtual void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IWorld world, IRemoteClient user)
        {
            // This space intentionally left blank
        }

        public virtual void ItemUsedOnNothing(ItemStack item, IWorld world, IRemoteClient user)
        {
            // This space intentionally left blank
        }
    }
}