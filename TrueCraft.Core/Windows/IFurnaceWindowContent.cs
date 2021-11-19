using System;

namespace TrueCraft.Core.Windows
{
    public interface IFurnaceWindowContent : IWindowContent
    {
        ISlots Ingredient { get; }

        ISlots Fuel { get; }

        ISlots Output { get; }
    }
}
