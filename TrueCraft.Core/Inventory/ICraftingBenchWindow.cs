using System;

namespace TrueCraft.Core.Inventory
{
    public interface ICraftingBenchWindow<T> : IWindow<T> where T : ISlot
    {
        ICraftingArea<T> CraftingArea { get; }
    }
}
