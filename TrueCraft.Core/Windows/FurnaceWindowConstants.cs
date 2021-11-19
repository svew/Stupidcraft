using System;

namespace TrueCraft.Core.Windows
{
    public static class FurnaceWindowConstants
    {
        // NOTE: these values must match the order in which the slots
        //    collections are added in the constructors.
        public enum AreaIndices
        {
            Ingredient = 0,
            Fuel = 1,
            Output = 2,
            Main = 3,
            Hotbar = 4
        }

        public static ISlots[] Areas(ISlots mainInventory, ISlots hotBar)
        {
            return new[]
                {
                    new Slots(1, 1, 1),  // Ingredients
                    new Slots(1, 1, 1),  // Fuel
                    new Slots(1, 1, 1),  // Output
                    mainInventory,
                    hotBar
                };
        }
    }
}
