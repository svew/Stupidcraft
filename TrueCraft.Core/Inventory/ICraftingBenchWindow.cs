using System;

namespace TrueCraft.Core.Inventory
{
    public interface ICraftingBenchWindow<T> : IWindow<T> where T : ISlot
    {
        ICraftingArea<T> CraftingArea { get; }

        /// <summary>
        /// Gets the Slot Index (within the Window) of the Output Slot.
        /// </summary>
        int CraftingOutputSlotIndex { get; }
    }
}
