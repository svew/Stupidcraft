using System;

namespace TrueCraft.Core.Inventory
{
    public interface IInventoryWindow<T> : IWindow<T> where T : ISlot
    {
        /// <summary>
        /// Gets the 2x2 Crafting Grid which is part of the Inventory Window.
        /// </summary>
        public ICraftingArea<T> CraftingGrid { get; }

        public ISlots<T> Armor { get; }
    }
}
