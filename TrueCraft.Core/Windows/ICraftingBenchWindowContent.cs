using System;
using TrueCraft.API;
using TrueCraft.API.Windows;

namespace TrueCraft.Core.Windows
{
    public interface ICraftingBenchWindowContent : IWindowContent
    {
        ISlots CraftingGrid { get; }

        ItemStack [] ClearInputs();
    }
}
