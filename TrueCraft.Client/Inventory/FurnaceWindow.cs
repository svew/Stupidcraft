using System;
using fNbt;
using TrueCraft.Client.Handlers;
using TrueCraft.Client.Modules;
using TrueCraft.Core;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Logic;
using TrueCraft.Core.World;

namespace TrueCraft.Client.Inventory
{
    public class FurnaceWindow : TrueCraft.Core.Inventory.FurnaceWindow<ISlot>, IClickHandler
    {
        public FurnaceWindow(IItemRepository itemRepository,
            ISlotFactory<ISlot> slotFactory, sbyte windowID,
            ISlots<ISlot> mainInventory, ISlots<ISlot> hotBar,
            IWorld world, GlobalVoxelCoordinates location) :
            base(itemRepository, slotFactory, windowID, mainInventory, hotBar)
        {
        }

        public override void SetSlots(ItemStack[] slotContents)
        {
#if DEBUG
            if (slotContents.Length != Count)
                throw new ApplicationException($"{nameof(slotContents)}.Length has value of {slotContents.Length}, but {Count} was expected.");
#endif
            int index = 0;
            for (int j = 0, jul = Slots.Length; j < jul; j++)
                for (int k = 0, kul = Slots[j].Count; k < kul; k++)
                {
                    Slots[j][k].Item = slotContents[index];
                    index++;
                }
        }

        /// <inheritdoc />
        public ActionConfirmation HandleClick(int slotIndex, bool rightClick, bool shiftClick, IHeldItem heldItem)
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

        protected ActionConfirmation HandleLeftClick(int slotIndex, IHeldItem heldItem)
        {
            if (IsOutputSlot(slotIndex))
            {
                // can only remove from output slot.
                ItemStack output = this[slotIndex];

                // It is a No-Op if either the output slot is empty or the output
                // is not compatible with the item in hand.
                // It is assumed that Beta 1.7.3 sends a window click anyway in this case.
                // However, the client can be compatible if we don't bother the
                // server about such things.
                if (output.Empty || !output.CanMerge(heldItem.HeldItem))
                    return null;

                return ActionConfirmation.GetActionConfirmation(() =>
                {
                    short itemID = output.ID;
                    short metadata = output.Metadata;
                    NbtCompound nbt = output.Nbt;
                    int maxStack = ItemRepository.GetItemProvider(itemID).MaximumStack;
                    int numToPickUp = Math.Min(maxStack - heldItem.HeldItem.Count, output.Count);

                    heldItem.HeldItem = new ItemStack(itemID, (sbyte)(heldItem.HeldItem.Count + numToPickUp), metadata, nbt);
                    this[slotIndex] = output.GetReducedStack(numToPickUp);
                });
            }

            // Play-testing of Beta 1.7.3 shows
            //  - Anything can be placed in the Fuel Slot.
            //  - Anything can be placed in the Ingredient Slot
            ItemStack slotContent = this[slotIndex];

            if (slotContent.Empty || heldItem.HeldItem.Empty || !slotContent.CanMerge(heldItem.HeldItem))
            {
                return ActionConfirmation.GetActionConfirmation(() =>
                {
                    this[slotIndex] = heldItem.HeldItem;
                    heldItem.HeldItem = slotContent;
                });
            }
            else
            {
                return ActionConfirmation.GetActionConfirmation(() =>
                {
                    int maxStack = ItemRepository.GetItemProvider(heldItem.HeldItem.ID).MaximumStack;
                    int numToPlace = Math.Min(maxStack - slotContent.Count, heldItem.HeldItem.Count);
                    this[slotIndex] = new ItemStack(slotContent.ID, (sbyte)(slotContent.Count + numToPlace),
                        slotContent.Metadata, slotContent.Nbt);
                    heldItem.HeldItem = heldItem.HeldItem.GetReducedStack(numToPlace);
                });
            }
        }

        protected ActionConfirmation HandleShiftLeftClick(int slotIndex, IHeldItem heldItem)
        {
            // TODO
            throw new NotImplementedException();
        }

        protected ActionConfirmation HandleRightClick(int slotIndex, IHeldItem heldItem)
        {
            // TODO
            throw new NotImplementedException();
        }

        protected ActionConfirmation HandleShiftRightClick(int slotIndex, IHeldItem heldItem)
        {
            return HandleShiftLeftClick(slotIndex, heldItem);
        }

    }
}
