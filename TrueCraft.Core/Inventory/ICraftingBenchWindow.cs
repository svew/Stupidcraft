using System;

namespace TrueCraft.Core.Inventory
{
    public interface ICraftingBenchWindow<T> : IWindow<T> where T : ISlot
    {
        /// <summary>
        /// Gets the 3x3 Crafting Grid (plus output slot) of the Crafting Bench Window.
        /// </summary>
        ICraftingArea<T> CraftingGrid { get; }

        /// <summary>
        /// Gets the Slot Index (within the Window) of the Output Slot.
        /// </summary>
        int CraftingOutputSlotIndex { get; }
    }
}
