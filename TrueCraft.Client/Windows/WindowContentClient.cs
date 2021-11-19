using System;
using TrueCraft.Core.Logic;
using TrueCraft.Client.Handlers;
using TrueCraft.Client.Modules;
using TrueCraft.Core.Windows;

namespace TrueCraft.Client.Windows
{
    public abstract class WindowContentClient : WindowContent, IWindowContentClient
    {
        public WindowContentClient(ISlots[] slotAreas, IItemRepository itemRepository) :
            base(slotAreas, itemRepository)
        {
        }

        /// <inheritdoc />
        public virtual ActionConfirmation HandleClick(int slotIndex, bool rightClick, bool shiftClick, IHeldItem heldItem)
        {
            if (rightClick)
            {
                if (shiftClick)
                    return HandleShiftRightClick(slotIndex, heldItem);
                else
                    return HandleRightClick(slotIndex, heldItem);
            }
            else
            {
                if (shiftClick)
                    return HandleShiftLeftClick(slotIndex, heldItem);
                else
                    return HandleLeftClick(slotIndex, heldItem);
            }
        }

        protected abstract ActionConfirmation HandleLeftClick(int slotIndex, IHeldItem heldItem);

        protected abstract ActionConfirmation HandleShiftLeftClick(int slotIndex, IHeldItem heldItem);

        protected abstract ActionConfirmation HandleRightClick(int slotIndex, IHeldItem heldItem);

        protected virtual ActionConfirmation HandleShiftRightClick(int slotIndex, IHeldItem heldItem)
        {
            return HandleShiftLeftClick(slotIndex, heldItem);
        }
    }
}
