using System;
using TrueCraft.API.Logic;
using TrueCraft.API.Windows;

namespace TrueCraft.Core.Windows
{
    public static class CraftingBenchWindowConstants
    {
        // NOTE: These values must match the order in which the Slot collections
        //  are added in the constructors.
        public enum AreaIndices
        {
            Crafting = 0,
            Main = 1,
            Hotbar = 2
        }

        public static ISlots[] Areas(ISlots mainInventory, ISlots hotBar,
            ICraftingRepository craftingRepository)
        {
            return new[]
                {
                new CraftingWindowContent(craftingRepository, 3, 3),
                mainInventory,
                hotBar
                };
        }
    }
}
