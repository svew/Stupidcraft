using System;

namespace TrueCraft.API.Windows
{
    /// <summary>
    /// Represents a collection of related item slots.
    /// </summary>
    /// <remarks>
    /// It represents a data storage that supports a window area on the GUI.
    /// </remarks>
    public interface ISlots : IDisposable
    {
        event EventHandler<WindowChangeEventArgs> WindowChange;

        /// <summary>
        /// Gets the total number of slots within the Window Area.
        /// </summary>
        /// <remarks>
        /// This is usually, but not always, the product of the
        /// Length and Width.
        /// </remarks>
        int Count { get; }

        /// <summary>
        /// Gets the width of the rectangular portion of the Window Area
        /// in slots.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the height of the rectangular portion of the Window Area
        /// in slots.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the Array containing the ItemStack objects in this Window Area.
        /// </summary>
        ItemStack[] Items { get; }

        /// <summary>
        /// Gets or sets the ItemStack at the given index within this Window Area.
        /// </summary>
        /// <param name="index">The index within this Window Area.  The first
        /// ItemStack is at index 0, not StartIndex.</param>
        /// <returns></returns>
        ItemStack this[int index] { get; set; }

        /// <summary>
        /// Adds the given ItemStack to these slots, merging with established slots if possible.
        /// </summary>
        /// <param name="items">The ItemStack to merge into these Slots.</param>
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
    }
}
