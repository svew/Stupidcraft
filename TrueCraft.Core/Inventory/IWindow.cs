using System;
using System.Collections.Generic;
using TrueCraft.Core.Windows;

namespace TrueCraft.Core.Inventory
{
    public interface IWindow<T> where T : ISlot
    {
        public event EventHandler<WindowClosedEventArgs> WindowClosed;

        ItemStack this[int index] { get; set; }

        void SetSlots(ItemStack[] slotContents);

        sbyte WindowID { get; }
        string Name { get; }
        WindowType Type { get; }

        /// <summary>
        /// Gets the total number of Slots in this Window.
        /// </summary>
        /// <remarks>This does not match the Number of Slots Field in the
        /// OpenWindowPacket.  This includes the Main Inventory and Hotbar.</remarks>
        int Count { get; }

        /// <summary>
        /// Gets the collection of Slots representing the Player's Main Inventory
        /// </summary>
        ISlots<T> MainInventory { get; }

        /// <summary>
        /// Gets the Slot Index (within the Window) of the first Slot of the Main Inventory Area.
        /// </summary>
        int MainSlotIndex { get; }

        /// <summary>
        /// Gets the collection of Slots representing the Player's Hotbar.
        /// </summary>
        ISlots<T> Hotbar { get; }

        /// <summary>
        /// Gets the Slot Index (within the Window) of the first Slot of the Hotbar Area.
        /// </summary>
        int HotbarSlotIndex { get; }

        bool IsOutputSlot(int slotIndex);

        /// <summary>
        /// Adds the given ItemStack to the Slots collections in this window.
        /// </summary>
        /// <param name="item">The ItemStack to add.</param>
        /// <returns>Any residual items which did not fit.</returns>
        ItemStack StoreItemStack(ItemStack item);

        /// <summary>
        /// Adds the given ItemStack to the Slots collections in this window.
        /// </summary>
        /// <param name="item">The ItemStack to add.</param>
        /// <param name="affectedSlots">Returns a List of affected Slot Indices.</param>
        /// <param name="newItems">Returns the new contents of each affected Slot.</param>
        /// <returns>Any residual items which did not fit.</returns>
        ItemStack StoreItemStack(ItemStack item, out List<int> affectedSlotIndices, out List<ItemStack> newItems);
    }
}