using System;

namespace TrueCraft.Core.Windows
{
    public interface ICraftingBenchWindowContent : IWindowContent
    {
        ISlots CraftingGrid { get; }

        ItemStack [] ClearInputs();
    }
}
