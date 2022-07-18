using System;
using TrueCraft.Core.World;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Entities;
using System.Collections.Generic;

namespace TrueCraft.Core.Logic
{
    public interface IItemProvider
    {
        /// <summary>
        /// Gets the Item ID of this Item Provider.
        /// </summary>
        short ID { get; }

        /// <summary>
        /// Gets the maximum number of items of this type that can Stack together.
        /// </summary>
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

        /// <summary>
        /// Gets the location of this item's icon texture within the items.png spritesheet.
        /// </summary>
        /// <param name="metadata">The metadata which may affect the item's texture.</param>
        /// <returns>A tuple containing x- (Item1) and y- (Item2) components of the
        /// location of the texture within the items.png file.  The units are
        /// icon textures.</returns>
        Tuple<int, int>? GetIconTexture(byte metadata);

        /// <summary>
        /// Gets an enumerable over any metadata which affects rendering.
        /// </summary>
        IEnumerable<short> VisibleMetadata { get; }
    }
}
