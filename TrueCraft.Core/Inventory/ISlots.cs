using System;
using System.Collections.Generic;

namespace TrueCraft.Core.Inventory
{
    /// <summary>
    /// A Collection of Slots within a window where the slots have a common purpose.
    /// For example, the Main Inventory or the HotBar.
    /// </summary>
    public interface ISlots<T> : IList<T> where T : ISlot
    {
        /// <summary>
        /// Adds the given ItemStack to these slots, merging with established slots if possible.
        /// </summary>
        /// <param name="item">The ItemStack to merge into these Slots.</param>
        /// <param name="topUpOnly">True if only partially filled slots should be topped up.
        /// No empty slots will be used.
        /// False to use an empty slot, if some Items do not fit into an already
        /// used Slot.</param>
        /// <returns>
        /// An ItemStack containing any leftover items that would not fit.
        /// If all given Items fit in these Slots, then ItemStack.EmptyStack will
        /// be returned.
        /// </returns>
        ItemStack StoreItemStack(ItemStack item, bool topUpOnly);

        /// <summary>
        /// Adds the given ItemStack to these slots, merging with established slots if possible.
        /// </summary>
        /// <param name="item">The ItemStack to merge into these Slots.</param>
        /// <param name="topUpOnly">True if only partially filled slots should be topped up.
        /// No empty slots will be used.
        /// False to use an empty slot, if some Items do not fit into an already
        /// used Slot.</param>
        /// <param name="affectedSlots">A List of affected Slot Indices.</param>
        /// <param name="newItems">The new contents of each affected Slot.</param>
        /// <returns>
        /// An ItemStack containing any leftover items that would not fit.
        /// If all given Items fit in these Slots, then ItemStack.EmptyStack will
        /// be returned.
        /// </returns>
        ItemStack StoreItemStack(ItemStack item, bool topUpOnly, out List<int> affectedSlots, out List<ItemStack> newItems);

        /// <summary>
        /// Gets the number of slots across when this Slots collection is displayed.
        /// </summary>
        /// <remarks>Note: this should be client-side only.</remarks>
        int Width { get; }
    }
}
