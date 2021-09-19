using System;
using TrueCraft.API.Windows;
using TrueCraft.API.Logic;
using TrueCraft.API;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Items;
using TrueCraft.Core.Windows;
using TrueCraft.Client.Handlers;
using TrueCraft.Client.Modules;

namespace TrueCraft.Client.Windows
{
    public class InventoryWindowContentClient : WindowContentClient, IInventoryWindowContent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainInventory"></param>
        /// <param name="hotBar"></param>
        /// <param name="armor"></param>
        /// <param name="craftingGrid"></param>
        public InventoryWindowContentClient(ISlots mainInventory, ISlots hotBar,
            ISlots armor, ISlots craftingGrid) :
            base(InventoryWindowConstants.Areas(mainInventory, hotBar, armor, craftingGrid),
               BlockProvider.ItemRepository)
        {
            CraftingOutputIndex = 0;
            ArmorIndex = CraftingOutputIndex + craftingGrid.Count;
            MainIndex = ArmorIndex + armor.Count;
            HotbarIndex = MainIndex + mainInventory.Count;
        }

        public int CraftingOutputIndex { get; }
        public int ArmorIndex { get; }
        public int MainIndex { get; }
        public int HotbarIndex { get; }

        #region Properties

        public override string Name
        {
            get
            {
                return "Inventory";
            }
        }

        public override WindowType Type
        {
            get
            {
                return WindowType.Inventory;
            }
        }

        public override bool IsOutputSlot(int slotIndex)
        {
            return slotIndex == InventoryWindowConstants.CraftingOutputIndex;
        }

        public ICraftingArea CraftingGrid { get => (ICraftingArea)SlotAreas[(int)InventoryWindowConstants.AreaIndices.Crafting]; }

        public ISlots Armor { get => SlotAreas[(int)InventoryWindowConstants.AreaIndices.Armor]; }

        public override ISlots MainInventory { get => SlotAreas[(int)InventoryWindowConstants.AreaIndices.Main]; }

        public override ISlots Hotbar { get => SlotAreas[(int)InventoryWindowConstants.AreaIndices.Hotbar]; }

        public override bool IsPlayerInventorySlot(int slotIndex)
        {
            return slotIndex >= SlotAreas[(int)InventoryWindowConstants.AreaIndices.Crafting].Count +
                SlotAreas[(int)InventoryWindowConstants.AreaIndices.Armor].Count;
        }

        #endregion

        protected override ISlots GetLinkedArea(int index, ItemStack slot)
        {
            if (index == 0 || index == 1 || index == 3)
                return MainInventory;
            return Hotbar;
        }

        public ItemStack PickUpStack(ItemStack item)
        {
            ItemStack remaining = MainInventory.StoreItemStack(item, true);
            if (remaining.Empty)
                return ItemStack.EmptyStack;

            remaining = Hotbar.StoreItemStack(remaining, false);
            if (remaining.Empty)
                return ItemStack.EmptyStack;

            return MainInventory.StoreItemStack(remaining, false);
        }

        public override ItemStack StoreItemStack(ItemStack slot, bool topUpOnly)
        {
            throw new NotImplementedException();
        }

        public override ItemStack MoveItemStack(int index)
        {
            InventoryWindowConstants.AreaIndices src = (InventoryWindowConstants.AreaIndices)GetAreaIndex(index);

            if (src == InventoryWindowConstants.AreaIndices.Main)
            {
                return Hotbar.StoreItemStack(this[index], false);
            }
            else if (src == InventoryWindowConstants.AreaIndices.Hotbar)
            {
                return MainInventory.StoreItemStack(this[index], false);
            }
            else
            {
                ItemStack remaining = MainInventory.StoreItemStack(this[index], true);
                if (remaining.Empty)
                    return remaining;

                remaining = Hotbar.StoreItemStack(remaining, false);
                if (remaining.Empty)
                    return remaining;

                return MainInventory.StoreItemStack(remaining, false);
            }
        }

        protected override ActionConfirmation HandleLeftClick(int slotIndex, IHeldItem heldItem)
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
                        sbyte maxItems = BlockProvider.ItemRepository.GetItemProvider(inHand.ID).MaximumStack;
                        int totalItems = inHand.Count + this[slotIndex].Count;
                        if (totalItems > maxItems)
                        {   // There are too many items, so the mouse pointer
                            // becomes a full stack, and the output slot retains the
                            // remaining items.
                            return ActionConfirmation.GetActionConfirmation(() =>
                            {
                                heldItem.HeldItem = new ItemStack(inHand.ID, maxItems, inHand.Metadata, inHand.Nbt);
                                this[slotIndex] = new ItemStack(inHand.ID, (sbyte)(totalItems - maxItems), inHand.Metadata, inHand.Nbt);
                            });
                        }
                        else
                        {   // There's enough room to pick up everything, so do it.
                            return ActionConfirmation.GetActionConfirmation(() =>
                            {
                                heldItem.HeldItem = new ItemStack(inHand.ID, (sbyte)totalItems, inHand.Metadata, inHand.Nbt);
                                this[slotIndex] = ItemStack.EmptyStack;
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
                        heldItem.HeldItem = this[slotIndex];
                        this[slotIndex] = ItemStack.EmptyStack;
                    });
                }
            }

            if (!inHand.Empty)
            {
                // Is the slot compatible
                if (inHand.CanMerge(this[slotIndex]))
                {
                    // How many Items can be placed?
                    sbyte maxItems = BlockProvider.ItemRepository.GetItemProvider(inHand.ID).MaximumStack;
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

        protected override ActionConfirmation HandleShiftLeftClick(int slotIndex, IHeldItem heldItem)
        {
            return ActionConfirmation.GetActionConfirmation(() =>
            {
                this[slotIndex] = MoveItemStack(slotIndex);
            });
        }

        protected override ActionConfirmation HandleRightClick(int slotIndex, IHeldItem heldItem)
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
                IItemProvider itemInOutput = ItemRepository.GetItemProvider(output.ID);
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
                    int maxStack = ItemRepository.GetItemProvider(heldItem.HeldItem.ID).MaximumStack;
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
    }
}
