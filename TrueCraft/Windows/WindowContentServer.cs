using System;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.API.Windows;
using TrueCraft.Core.Windows;

namespace TrueCraft.Windows
{
    public abstract class WindowContentServer : WindowContent, IWindowContentServer
    {
        protected WindowContentServer(ISlots[] slotAreas, IItemRepository itemRepository) :
            base(slotAreas, itemRepository)
        {

        }

        /// <inheritdoc />
        public virtual bool HandleClick(int slotIndex, bool right, bool shift, ref ItemStack itemStaging)
        {
            if (right)
            {
                if (shift)
                    return HandleShiftRightClick(slotIndex, ref itemStaging);
                else
                    return HandleRightClick(slotIndex, ref itemStaging);
            }
            else
            {
                if (shift)
                    return HandleShiftLeftClick(slotIndex, ref itemStaging);
                else
                    return HandleLeftClick(slotIndex, ref itemStaging);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotIndex"></param>
        /// <param name="itemStaging"></param>
        /// <remarks>
        /// With nothing in hand, picks up entire stack.
        /// With something in hand, swaps with entire (incompatible) stack.
        /// With something in hand, places as much as possible in compatible stack.
        /// </remarks>
        protected abstract bool HandleLeftClick(int slotIndex, ref ItemStack itemStaging);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Whether or not you have something in hand, moves entire stack.
        /// Target of move is dependent upon which window is displayed.
        /// </remarks>
        protected abstract bool HandleShiftLeftClick(int slotIndex, ref ItemStack itemStaging);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// With nothing in hand, picks up half the stack.  For odd numbers,
        /// the extra one is picked up.
        /// With something in hand, places one item in a compatible slot.
        /// For a slot with something different in it, swaps the entire stack.
        /// </remarks>
        protected abstract bool HandleRightClick(int slotIndex, ref ItemStack itemStaging);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Acts the same as Shift-Left-Click.
        /// </remarks>
        protected virtual bool HandleShiftRightClick(int slotIndex, ref ItemStack itemStaging)
        {
            return HandleShiftLeftClick(slotIndex, ref itemStaging);
        }
    }
}
