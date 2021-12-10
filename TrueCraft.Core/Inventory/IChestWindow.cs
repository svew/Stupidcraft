using System;

namespace TrueCraft.Core.Inventory
{
    public interface IChestWindow<T> : IWindow<T> where T : ISlot
    {
        /// <summary>
        /// Gets the collection of slots containing the Chest's contents.
        /// </summary>
        ISlots<T> ChestInventory { get; }

        /// <summary>
        /// Gets whether or not the Chest is a double Chest.
        /// </summary>
        bool DoubleChest { get; }

        /// <summary>
        /// Gets the Slot Index (within the Window) of the first Slot of the Chest Area.
        /// </summary>
        int ChestSlotIndex { get; }

        /// <summary>
        /// Gets the Slot Index (within the Window) of the first Slot of the Main Inventory Area.
        /// </summary>
        int MainSlotIndex { get; }

        /// <summary>
        /// Gets the Slot Index (within the Window) of the first Slot of the Hotbar Area.
        /// </summary>
        int HotbarSlotIndex { get; }
    }
}
