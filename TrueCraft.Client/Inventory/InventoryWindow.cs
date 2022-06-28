using System;
using TrueCraft.Client.Handlers;
using TrueCraft.Client.Modules;
using TrueCraft.Core;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Windows;

namespace TrueCraft.Client.Inventory
{
    public class InventoryWindow : InventoryWindow<ISlot>, IClickHandler
    {
        public InventoryWindow(IItemRepository itemRepository,
            ICraftingRepository craftingRepository, ISlotFactory<ISlot> slotFactory,
            ISlots<ISlot> mainInventory, ISlots<ISlot> hotBar) :
            base(itemRepository, craftingRepository, slotFactory,
                mainInventory, hotBar)
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
        public virtual ActionConfirmation? HandleClick(int slotIndex, bool rightClick, bool shiftClick, IHeldItem heldItem)
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

        protected ActionConfirmation? HandleLeftClick(int slotIndex, IHeldItem heldItem)
        {
            ItemStack inHand = heldItem.HeldItem;

            if (IsOutputSlot(slotIndex))
            {
                if (!inHand.Empty)
                {
                    if (inHand.CanMerge(this[slotIndex]))
                    {
                        // The mouse pointer has some items in it, and they
                        // are compatible with the output
                        sbyte maxItems = ItemRepository.GetItemProvider(inHand.ID)!.MaximumStack;    // inHand is known to not be Empty
                        int totalItems = inHand.Count + this[slotIndex].Count;
                        if (totalItems > maxItems)
                        {   // There are too many items.  This is a No-OP.
                            // The client can be compatible by not bothering
                            // the server with a No-Op.
                            return null;
                        }
                        else
                        {   // There's enough room to pick some up, so pick up o
                            // Recipe's worth.
                            return ActionConfirmation.GetActionConfirmation(() =>
                            {
                                heldItem.HeldItem = new ItemStack(inHand.ID, (sbyte)totalItems, inHand.Metadata, inHand.Nbt);
                                CraftingGrid.TakeOutput();
                            });
                        }
                    }
                    else
                    {   // The mouse pointer contains an item incompatible with
                        // the output, so we cannot complete this operation.
                        return null;
                    }
                }
                else
                {
                    // If the mouse pointer is empty, just pick up everything.
                    return ActionConfirmation.GetActionConfirmation(() =>
                    {
                        heldItem.HeldItem = CraftingGrid.TakeOutput();
                    });
                }
            }

            if (!inHand.Empty)
            {
                // Is the slot compatible
                if (inHand.CanMerge(this[slotIndex]))
                {
                    // How many Items can be placed?
                    sbyte maxItems = ItemRepository.GetItemProvider(inHand.ID)!.MaximumStack;    // inHand is known to not be Empty
                    int totalItems = inHand.Count + this[slotIndex].Count;
                    ItemStack old = this[slotIndex];
                    if (totalItems > maxItems)
                    {   // Fill the Slot to the max, retaining remaining items.
                        return ActionConfirmation.GetActionConfirmation(() =>
                        {
                            this[slotIndex] = new ItemStack(old.ID, maxItems, old.Metadata, old.Nbt);
                            heldItem.HeldItem = new ItemStack(inHand.ID, (sbyte)(totalItems - maxItems), inHand.Metadata, inHand.Nbt);
                        });
                    }
                    else
                    {   // Place all items, the mouse pointer becomes empty.
                        return ActionConfirmation.GetActionConfirmation(() =>
                        {
                            this[slotIndex] = new ItemStack(heldItem.HeldItem.ID, (sbyte)totalItems, heldItem.HeldItem.Metadata, heldItem.HeldItem.Nbt);
                            heldItem.HeldItem = ItemStack.EmptyStack;
                        });
                    }
                }
                else
                {   // The slot is not compatible with the mouse pointer, so
                    // swap them.
                    return ActionConfirmation.GetActionConfirmation(() =>
                    {
                        heldItem.HeldItem = this[slotIndex];
                        this[slotIndex] = inHand;
                    });
                }
            }
            else
            {   // The mouse pointer is empty, so pick up everything.
                return ActionConfirmation.GetActionConfirmation(() =>
                {
                    heldItem.HeldItem = this[slotIndex];
                    this[slotIndex] = ItemStack.EmptyStack;
                });
            }
        }

        protected ActionConfirmation? HandleShiftLeftClick(int slotIndex, IHeldItem heldItem)
        {
            if (IsOutputSlot(slotIndex))
            {
                ItemStack output = this[slotIndex];
                if (output.Empty)
                    // This is a No-Op.
                    return null;

                return ActionConfirmation.GetActionConfirmation(() =>
                {
                    // Q: What if we craft 4 sticks, but only have room for 2?
                    // Play-testing this in Beta 1.7.3 shows that the excess sticks
                    // simply disappeared.

                    output = this[slotIndex];
                    ItemStack remaining = MainInventory.StoreItemStack(output, true);
                    remaining = Hotbar.StoreItemStack(remaining, false);
                    remaining = MainInventory.StoreItemStack(remaining, false);
                    if (remaining.Count != output.Count)
                        CraftingGrid.TakeOutput();
                });
            }

            return ActionConfirmation.GetActionConfirmation(() =>
            {
                this[slotIndex] = MoveItemStack(slotIndex);
            });
        }

        private ItemStack MoveItemStack(int fromSlotIndex)
        {
            AreaIndices src = (AreaIndices)GetAreaIndex(fromSlotIndex);

            if (src == AreaIndices.Main)
            {
                return Hotbar.StoreItemStack(this[fromSlotIndex], false);
            }
            else if (src == AreaIndices.Hotbar)
            {
                return MainInventory.StoreItemStack(this[fromSlotIndex], false);
            }
            else
            {
                ItemStack remaining = MainInventory.StoreItemStack(this[fromSlotIndex], true);
                if (remaining.Empty)
                    return remaining;

                remaining = Hotbar.StoreItemStack(remaining, false);
                if (remaining.Empty)
                    return remaining;

                return MainInventory.StoreItemStack(remaining, false);
            }
        }

        protected ActionConfirmation? HandleRightClick(int slotIndex, IHeldItem heldItem)
        {
            if (IsOutputSlot(slotIndex))
            {
                ItemStack output = this[slotIndex];

                // Clicking on an empty output, or an output which cannot
                // be merged with the items in hand is a No-Op.
                // Packet-sniffing shows that Beta 1.7.3 does send a Window Click
                // Packet to the server.  The client can be compatible without
                // bothering the server with such No-Ops.
                if (output.Empty || !heldItem.HeldItem.CanMerge(output))
                    return null;

                // If we have room for it, pick up one Recipe's worth of output.
                IItemProvider itemInOutput = ItemRepository.GetItemProvider(output.ID)!;   // output is known to not be Empty
                int maxHandStack = itemInOutput.MaximumStack;

                if (!output.Empty && heldItem.HeldItem.CanMerge(output) && heldItem.HeldItem.Count + output.Count <= maxHandStack)
                {
                    return ActionConfirmation.GetActionConfirmation(() =>
                    {
                        output = CraftingGrid.TakeOutput();
                        heldItem.HeldItem = new ItemStack(output.ID, (sbyte)(output.Count + heldItem.HeldItem.Count),
                            output.Metadata, output.Nbt);
                    });
                }

                // No room to pick up a compatible stack is a No-Op.
                // The client can be compatible without bothering the server
                // with a No-Op.
                return null;
            }

            if (!heldItem.HeldItem.Empty)
            {
                // If the hand is full, and the slot contents are not compatible, swap them.
                if (this[slotIndex].CanMerge(heldItem.HeldItem))
                {
                    // The hand holds something, and the slot contents are compatible, place one item.
                    int maxStack = ItemRepository.GetItemProvider(heldItem.HeldItem.ID)!.MaximumStack;   // heldItem is known to not be Empty
                    if (maxStack > this[slotIndex].Count)
                        return ActionConfirmation.GetActionConfirmation(() =>
                        {
                            this[slotIndex] = new ItemStack(heldItem.HeldItem.ID, (sbyte)(this[slotIndex].Count + 1), heldItem.HeldItem.Metadata, heldItem.HeldItem.Nbt);
                            heldItem.HeldItem = heldItem.HeldItem.GetReducedStack(1);
                        });

                    // Right-clicking on a full compatible slot is a No-Op.
                    // The client can be compatible without bothering the server for No-Ops.
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
            else
            {
                // If the hand is empty, pick up half the stack.
                ItemStack slotContent = this[slotIndex];
                if (slotContent.Empty)
                    // Right-clicking an empty hand on an empty slot is a No-Op.
                    // The client can be compatible without sending No-Op window clicks.
                    return null;

                return ActionConfirmation.GetActionConfirmation(() =>
                {
                    int numToPickUp = slotContent.Count;
                    numToPickUp = numToPickUp / 2 + (numToPickUp & 0x0001);
                    heldItem.HeldItem = new ItemStack(slotContent.ID, (sbyte)numToPickUp, slotContent.Metadata, slotContent.Nbt);
                    this[slotIndex] = slotContent.GetReducedStack(numToPickUp);
                });
            }
        }

        protected ActionConfirmation? HandleShiftRightClick(int slotIndex, IHeldItem heldItem)
        {
            return HandleShiftLeftClick(slotIndex, heldItem);
        }
    }
}
