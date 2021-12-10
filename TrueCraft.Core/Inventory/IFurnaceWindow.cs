using System;

namespace TrueCraft.Core.Inventory
{
    public interface IFurnaceWindow<T> : IWindow<T> where T : ISlot
    {
        ISlots<T> Ingredient { get; }

        ISlots<T> Fuel { get; }

        ISlots<T> Output { get; }
    }
}
