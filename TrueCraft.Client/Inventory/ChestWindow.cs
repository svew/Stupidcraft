using System;
using TrueCraft.Client.Handlers;
using TrueCraft.Client.Modules;
using TrueCraft.Core;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Logic;
using TrueCraft.Core.World;

namespace TrueCraft.Client.Inventory
{
    public class ChestWindow : TrueCraft.Core.Inventory.ChestWindow<ISlot>, IClickHandler
    {
        public ChestWindow(IItemRepository itemRepository,
            ISlotFactory<ISlot> slotFactory, sbyte windowID,
            ISlots<ISlot> mainInventory, ISlots<ISlot> hotBar,
            IDimension dimension, GlobalVoxelCoordinates location, GlobalVoxelCoordinates otherHalf) :
            base(itemRepository, slotFactory, windowID, mainInventory, hotBar,
                otherHalf != null)
        {
        }

        public override void SetSlots(ItemStack[] slotContents)
        {
#if DEBUG
            if (slotContents.Length != Count)
                throw new ApplicationException($"{nameof(slotContents)}.Length has value of {slotContents.Length}, but {Count} was expected.");
#endif
            int index = 0;
            for (int j = 0, jul = Slots.Length; j < jul; j ++)
                for (int k = 0, kul = Slots[j].Count; k < kul; k ++)
                {
                    Slots[j][k].Item = slotContents[index];
                    index++;
                }
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

        protected ActionConfirmation HandleLeftClick(int slotIndex, IHeldItem heldItem)
        {
            if (heldItem.HeldItem.Empty)
            {
                // If the slot is also empty, this is a No-Op.
                // The client can be compatible without bothering the server about this.
                if (this[slotIndex].Empty)
                    return null;

                return ActionConfirmation.GetActionConfirmation(() =>
                {
                    heldItem.HeldItem = this[slotIndex];
                    this[slotIndex] = ItemStack.EmptyStack;
                });
            }
            else
            {
                if (this[slotIndex].Empty)
                {
                    return ActionConfirmation.GetActionConfirmation(() =>
                    {
                        this[slotIndex] = heldItem.HeldItem;
                        heldItem.HeldItem = ItemStack.EmptyStack;
                    });
                }

                if (heldItem.HeldItem.CanMerge(this[slotIndex]))
                {
                    int maxStack = ItemRepository.GetItemProvider(heldItem.HeldItem.ID).MaximumStack;
                    int numToPlace = Math.Min(maxStack - this[slotIndex].Count, heldItem.HeldItem.Count);
                    if (numToPlace > 0)
                        return ActionConfirmation.GetActionConfirmation(() =>
                        {
                            ItemStack slot = this[slotIndex];
                            this[slotIndex] = new ItemStack(slot.ID, (sbyte)(slot.Count + numToPlace), slot.Metadata, slot.Nbt);
                            heldItem.HeldItem = heldItem.HeldItem.GetReducedStack(numToPlace);
                        });

                    // Left-clicking on a full slot is a No-Op.
                    // The client can be compatible without bothering the server here.
                    return null;
                }
                else
                {
                    return ActionConfirmation.GetActionConfirmation(() =>
                    {
                        ItemStack tmp = this[slotIndex];
                        this[slotIndex] = heldItem.HeldItem;
                        heldItem.HeldItem = tmp;
                    });
                }
            }
        }

        /// <inheritdoc />
        protected ActionConfirmation HandleShiftLeftClick(int slotIndex, IHeldItem heldItem)
        {
            AreaIndices srcArea = (AreaIndices)GetAreaIndex(slotIndex);

            if (srcArea == AreaIndices.ChestArea)
            {
                return ActionConfirmation.GetActionConfirmation(() =>
                {
                    ItemStack remaining = Hotbar.StoreItemStack(this[slotIndex], true);
                    remaining = MainInventory.StoreItemStack(remaining, true);
                    remaining = Hotbar.StoreItemStack(remaining, false);
                    this[slotIndex] = MainInventory.StoreItemStack(remaining, false);
                });
            }
            else
            {
                return ActionConfirmation.GetActionConfirmation(() =>
                {
                    ItemStack remaining = this[slotIndex];
                    remaining = ChestInventory.StoreItemStack(remaining, true);
                    this[slotIndex] = ChestInventory.StoreItemStack(remaining, false);
                });
            }
        }

        /// <inheritdoc />
        protected ActionConfirmation HandleRightClick(int slotIndex, IHeldItem heldItem)
        {
            ItemStack stack = this[slotIndex];
            if (!heldItem.HeldItem.Empty)
            {
                if (stack.CanMerge(heldItem.HeldItem))
                {
                    int maxStack = ItemRepository.GetItemProvider(heldItem.HeldItem.ID).MaximumStack;
                    if (stack.Count < maxStack)
                    {
                        return ActionConfirmation.GetActionConfirmation(() =>
                        {
                            ItemStack held = heldItem.HeldItem;
                            this[slotIndex] = new ItemStack(held.ID, (sbyte)(stack.Count + 1), held.Metadata, held.Nbt);
                            heldItem.HeldItem = held.GetReducedStack(1);
                        });
                    }
                    else
                    {
                        // Right-click on compatible, but maxed-out stack.
                        // This is a No-Op.  There is no need for a compatible
                        // client to bother the server about this.
                        return null;
                    }
                }
                else
                {
                    // Right-click on an incompatible slot => exchange stacks.
                    return ActionConfirmation.GetActionConfirmation(() =>
                    {
                        this[slotIndex] = heldItem.HeldItem;
                        heldItem.HeldItem = stack;
                    });
                }
            }
            else
            {
                // Right-clicking an empty hand on an empty slot is a No-Op.
                // This is a No-Op.  There is no need for a compatible
                // client to bother the server about this.
                if (stack.Empty)
                    return null;

                return ActionConfirmation.GetActionConfirmation(() =>
                {
                    int cnt = stack.Count;
                    int numToPickUp = cnt / 2 + (cnt & 0x0001);

                    heldItem.HeldItem = new ItemStack(stack.ID, (sbyte)numToPickUp, stack.Metadata, stack.Nbt);
                    this[slotIndex] = stack.GetReducedStack(numToPickUp);
                });
            }
        }

        protected ActionConfirmation HandleShiftRightClick(int slotIndex, IHeldItem heldItem)
        {
            return HandleShiftLeftClick(slotIndex, heldItem);
        }
    }
}
