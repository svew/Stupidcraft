using System;
using TrueCraft.API.Windows;

namespace TrueCraft.Core.Windows
{
    public static class ChestWindowConstants
    {
        // NOTE: the values in this enum must match the order
        //  in which the slot collections are added in the
        //  ChestWindowContentClient and ChestWindowContentServer
        //  constructors.
        public enum AreaIndices
        {
            ChestArea = 0,
            MainArea = 1,
            HotBarArea = 2
        }

        public static ISlots[] Areas(ISlots mainInventory, ISlots hotBar, bool doubleChest)
        {
            return doubleChest ?
                new[]
                {
                    new Slots(2 * ChestLength, ChestWidth, 2 * ChestHeight), // Chest
                    mainInventory,
                    hotBar
                } :
                new[]
                {
                    new Slots(ChestLength, ChestWidth, ChestHeight), // Chest
                    mainInventory,
                    hotBar
                };
        }

        public const int ChestWidth = 9;
        public const int ChestHeight = 3;
        public const int ChestLength = ChestWidth * ChestHeight;
    }
}
