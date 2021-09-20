using System;
using TrueCraft.API.Windows;
using TrueCraft.API.Logic;
using TrueCraft.API;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Items;
using TrueCraft.Core.Windows;

namespace TrueCraft.Windows
{
    public class InventoryWindowContentServer : WindowContentServer, IInventoryWindowContent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainInventory"></param>
        /// <param name="hotBar"></param>
        /// <param name="armor"></param>
        /// <param name="craftingGrid"></param>
        public InventoryWindowContentServer(ISlots mainInventory, ISlots hotBar,
            ISlots armor, ISlots craftingGrid) :
            base(InventoryWindowConstants.Areas(mainInventory, hotBar, armor, craftingGrid),
               TrueCraft.Core.Logic.ItemRepository.Get())
        {
        }

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

        /// <inheritdoc />
        protected override bool HandleLeftClick(int slotIndex, ref ItemStack itemStaging)
        {
            if (IsOutputSlot(slotIndex))
            {
                if (!itemStaging.Empty)
                {
                    if (itemStaging.CanMerge(this[slotIndex]))
                    {
                        // The mouse pointer has some items in it, and they
                        // are compatible with the output
                        sbyte maxItems = ItemRepository.GetItemProvider(itemStaging.ID).MaximumStack;
                        int totalItems = itemStaging.Count + this[slotIndex].Count;
                        if (totalItems > maxItems)
                        {   // There are too many items, so the mouse pointer
                            // becomes a full stack, and the output slot retains the
                            // remaining items.
                            itemStaging = new ItemStack(itemStaging.ID, maxItems, itemStaging.Metadata, itemStaging.Nbt);
                            this[slotIndex] = new ItemStack(itemStaging.ID, (sbyte)(totalItems - maxItems), itemStaging.Metadata, itemStaging.Nbt);
                            return true;
                        }
                        else
                        {   // There's enough room to pick up everything, so do it.
                            itemStaging = new ItemStack(itemStaging.ID, (sbyte)totalItems, itemStaging.Metadata, itemStaging.Nbt);
                            this[slotIndex] = ItemStack.EmptyStack;
                            return true;
                        }
                    }
                    else
                    {   // The mouse pointer contains an item incompatible with
                        // the output, so we cannot complete this operation.
                        return false;
                    }
                }
                else
                {
                    // If the mouse pointer is empty, just pick up everything.
                    itemStaging = this[slotIndex];
                    this[slotIndex] = ItemStack.EmptyStack;
                    return true;
                }
            }

            if (!itemStaging.Empty)
            {
                // Is the slot compatible
                if (itemStaging.CanMerge(this[slotIndex]))
                {
                    // How many Items can be placed?
                    sbyte maxItems = ItemRepository.GetItemProvider(itemStaging.ID).MaximumStack;
                    int totalItems = itemStaging.Count + this[slotIndex].Count;
                    ItemStack old = this[slotIndex];
                    if (totalItems > maxItems)
                    {   // Fill the Slot to the max, retaining remaining items.
                        this[slotIndex] = new ItemStack(old.ID, maxItems, old.Metadata, old.Nbt);
                        itemStaging = new ItemStack(itemStaging.ID, (sbyte)(totalItems - maxItems), itemStaging.Metadata, itemStaging.Nbt);
                        return true;
                    }
                    else
                    {   // Place all items, the mouse pointer becomes empty.
                        this[slotIndex] = new ItemStack(itemStaging.ID, (sbyte)totalItems, itemStaging.Metadata, itemStaging.Nbt);
                        itemStaging = ItemStack.EmptyStack;
                        return true;
                    }
                }
                else
                {   // The slot is not compatible with the mouse pointer, so
                    // swap them.
                    ItemStack tmp = itemStaging;
                    itemStaging = this[slotIndex];
                    this[slotIndex] = tmp;
                    return true;
                }
            }
            else
            {   // The mouse pointer is empty, so pick up everything.
                itemStaging = this[slotIndex];
                this[slotIndex] = ItemStack.EmptyStack;
                return true;
            }
        }

        protected override bool HandleShiftLeftClick(int slotIndex, ref ItemStack itemStaging)
        {
            if (IsOutputSlot(slotIndex))
            {
                ItemStack output = this[slotIndex];
                if (output.Empty)
                    // This is a No-Op.
                    return true;

                // Q: What if we craft 4 sticks, but only have room for 2?
                // Play-testing this in Beta 1.7.3 shows that the excess sticks
                // simply disappeared.

                ItemStack remaining = StoreItemStack(output);
                if (remaining.Count != output.Count)
                    CraftingGrid.TakeOutput();

                return true;
            }

            this[slotIndex] = MoveItemStack(slotIndex);
            return true;
        }

        protected override bool HandleRightClick(int slotIndex, ref ItemStack itemStaging)
        {
            if (IsOutputSlot(slotIndex))
            {
                ItemStack output = this[slotIndex];
                if (output.Empty)
                    // It looks odd, but beta 1.7.3 client really does send a
                    // window click packet, and the server responds with
                    // accepted = true.  This is a No-Op.
                    return true;

                // If the item is not compatible with the hand, do nothing
                if (!itemStaging.CanMerge(output))
                    // It looks odd, but beta 1.7.3 client really does send a
                    // window click packet, and the server responds with
                    // accepted = true.  This is a No-Op.
                    return true;

                // Pick up one Recipe's worth of output.
                // Q: do we have room for it?
                IItemProvider itemInOutput = ItemRepository.GetItemProvider(output.ID);
                int maxHandStack = itemInOutput.MaximumStack;

                if (!output.Empty && itemStaging.CanMerge(output) && itemStaging.Count + output.Count <= maxHandStack)
                {
                    output = CraftingGrid.TakeOutput();
                    itemStaging = new ItemStack(output.ID, (sbyte)(output.Count + itemStaging.Count),
                        output.Metadata, output.Nbt);
                    return true;
                }

                return true;
            }

            if (!itemStaging.Empty)
            {
                if (this[slotIndex].CanMerge(itemStaging))
                {
                    // The hand holds something, and the slot contents are compatible, place one item.
                    int maxStack = ItemRepository.GetItemProvider(itemStaging.ID).MaximumStack;
                    if (maxStack > this[slotIndex].Count)
                    {
                        this[slotIndex] = new ItemStack(itemStaging.ID, (sbyte)(this[slotIndex].Count + 1), itemStaging.Metadata, itemStaging.Nbt);
                        itemStaging = itemStaging.GetReducedStack(1);
                        return true;
                    }
                    // Right-clicking on a full compatible slot is a No-Op.
                    // The Beta 1.7.3 client does send a Window Click and it
                    // is acknowledged by the server.
                    return true;
                }
                else
                {
                    // The slot contents are not compatible with the items in hand.
                    // Swap them.
                    ItemStack tmp = this[slotIndex];
                    this[slotIndex] = itemStaging;
                    itemStaging = tmp;
                    return true;
                }
            }
            else
            {
                // If the hand is empty, pick up half the stack.
                ItemStack slotContent = this[slotIndex];
                if (slotContent.Empty)
                    // Right-clicking an empty hand on an empty slot is a No-Op.
                    // Beta 1.7.3 does send a Window Click and it is acknowledged
                    // by the server.
                    return true;

                int numToPickUp = slotContent.Count;
                numToPickUp = numToPickUp / 2 + (numToPickUp & 0x0001);
                itemStaging = new ItemStack(slotContent.ID, (sbyte)numToPickUp, slotContent.Metadata, slotContent.Nbt);
                this[slotIndex] = slotContent.GetReducedStack(numToPickUp);
                return true;
            }
        }
    }
}
