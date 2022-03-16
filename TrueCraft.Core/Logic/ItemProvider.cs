using System;
using System.Collections.Generic;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic
{
    public abstract class ItemProvider : IItemProvider
    {
        private static List<short> _metadata;

        static ItemProvider()
        {
            _metadata = new List<short>(1);
            _metadata.Add(0);
        }

        public abstract short ID { get; }

        public abstract Tuple<int, int> GetIconTexture(byte metadata);

        public virtual sbyte MaximumStack { get { return 64; } }

        /// <inheritdoc />
        public virtual IEnumerable<short> VisibleMetadata => _metadata;

        /// <inheritdoc />
        public virtual string GetDisplayName(short metadata)
        {
            return string.Empty;
        }

        public virtual void ItemUsedOnEntity(ItemStack item, IEntity usedOn, IDimension dimension, IRemoteClient user)
        {
            // This space intentionally left blank
        }

        public virtual void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IDimension dimension, IRemoteClient user)
        {
            // This space intentionally left blank
        }

        public virtual void ItemUsedOnNothing(ItemStack item, IDimension dimension, IRemoteClient user)
        {
            // This space intentionally left blank
        }
    }
}