using System;
using TrueCraft.Client.Handlers;
using TrueCraft.Client.Modules;

namespace TrueCraft.Client.Inventory
{
    public interface IClickHandler
    {
        ActionConfirmation HandleClick(int slotIndex, bool rightClick, bool shiftClick, IHeldItem heldItem);
    }
}
