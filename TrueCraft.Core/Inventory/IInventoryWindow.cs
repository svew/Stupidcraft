using System;

namespace TrueCraft.Core.Inventory
{
    public interface IInventoryWindow<T> : IWindow<T> where T : ISlot
    {
        /// <summary>
        /// Gets the 2x2 Crafting Grid which is part of the Inventory Window.
        /// </summary>
        public ICraftingArea<T> CraftingGrid { get; }

        /// <summary>
        /// Gets the Slot Index (within the Window) of the Crafting Output Slot.
        /// </summary>
        /// <remarks>This is the first slot of the Crafting Area.</remarks>
        int CraftingOutputSlotIndex { get; }

        public ISlots<T> Armor { get; }

        /// <summary>
        /// Gets the Slot Index (withing the Window) of the first Slot of the Armor Area.
        /// </summary>
        int ArmorSlotIndex { get; }
    }
}
