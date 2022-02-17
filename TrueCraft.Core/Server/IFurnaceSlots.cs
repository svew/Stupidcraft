using System;
using TrueCraft.Core.Inventory;

namespace TrueCraft.Core.Server
{
    public interface IFurnaceSlots
    {
        IServerSlot IngredientSlot { get; }
        IServerSlot FuelSlot { get; }
        IServerSlot OutputSlot { get; }
    }
}
