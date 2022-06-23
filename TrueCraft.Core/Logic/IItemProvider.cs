using System;
using TrueCraft.Core.World;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Entities;
using System.Collections.Generic;

namespace TrueCraft.Core.Logic
{
    public interface IItemProvider
    {
        short ID { get; }
        sbyte MaximumStack { get; }

        /// <summary>
        /// Gets the name to be displayed to the Player for this item.
        /// </summary>
        /// <param name="metadata">The item's metadata.</param>
        /// <returns>A string for display to the Player.</returns>
        string GetDisplayName(short metadata);

        void ItemUsedOnNothing(ItemStack item, IDimension dimension, IRemoteClient user);
        void ItemUsedOnEntity(ItemStack item, IEntity usedOn, IDimension dimension, IRemoteClient user);
        void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IDimension dimension, IRemoteClient user);
        Tuple<int, int>? GetIconTexture(byte metadata);

        /// <summary>
        /// Gets an enumerable over any metadata which affects rendering.
        /// </summary>
        IEnumerable<short> VisibleMetadata { get; }
    }
}
