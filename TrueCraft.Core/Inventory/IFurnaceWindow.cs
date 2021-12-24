using System;

namespace TrueCraft.Core.Inventory
{
    public interface IFurnaceWindow<T> : IWindow<T> where T : ISlot
    {
        ISlots<T> Ingredient { get; }

        /// <summary>
        /// Gets the Slot Index (within the Window) of the Ingredient Slot.
        /// </summary>
        int IngredientSlotIndex { get; }

        ISlots<T> Fuel { get; }

        /// <summary>
        /// Gets the Slot Index (within the Window) of the Fuel Slot.
        /// </summary>
        int FuelSlotIndex { get; }

        ISlots<T> Output { get; }

        /// <summary>
        /// Gets the Slot Index (within the Window) of the Output Slot.
        /// </summary>
        int OutputSlotIndex { get; }
    }
}
