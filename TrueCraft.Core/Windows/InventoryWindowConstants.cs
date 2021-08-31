using System;
using TrueCraft.API.Windows;

namespace TrueCraft.Core.Windows
{
    public static class InventoryWindowConstants
    {
        // NOTE: the values in this enum must match the order in which
        //  the slots collections are added in the constructors.
        public enum AreaIndices
        {
            Crafting = 0,
            Armor = 1,
            Main = 2,
            Hotbar = 3
        }

        public static ISlots[] Areas(ISlots mainInventory, ISlots hotBar,
            ISlots armor, ISlots craftingGrid)
        {
            return new[]
                {
                    craftingGrid,
                    armor,
                    mainInventory,
                    hotBar
                };
        }

        public const int InventoryWidth = 9;
        public const int InventoryHeight = 3;
        public const int InventoryLength = InventoryWidth * InventoryHeight;

        //public const short HotbarIndex = 36;
        //public const short HotbarLength = 9;

        //public const short CraftingGridIndex = 1;
        public const short CraftingOutputIndex = 0;
        //public const short ArmorIndex = 5;
        //public const short MainIndex = 9;
    }
}
