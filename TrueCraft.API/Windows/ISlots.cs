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
        /// Gets the index (within the parent Window) of the first slot
        /// in this Window Area.
        /// </summary>
        /// <remarks>
        /// The parent Window contains a collection of Window Area objects.
        /// The parent Window has an indexer that indexes all Window Area
        /// objects within itself.  This StartIndex property indicates
        /// where this Window Area lies within the parent Window's indices.
        /// </remarks>
        int StartIndex { get; }

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

        // TODO: remove CopyTo by using references to the collections rather
        // than copying back and forth for the various windows.
        void CopyTo(ISlots area);

        int MoveOrMergeItem(int index, ItemStack item, ISlots from);
    }
}
