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

        public ISlots CraftingGrid { get => SlotAreas[(int)InventoryWindowConstants.AreaIndices.Crafting]; }

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

            switch (src)
            {
                case InventoryWindowConstants.AreaIndices.Crafting:
                case InventoryWindowConstants.AreaIndices.Armor:
                    return MoveToInventory(index);

                case InventoryWindowConstants.AreaIndices.Main:
                case InventoryWindowConstants.AreaIndices.Hotbar:
                    return MoveFromInventory(index);

                default:
                    throw new ApplicationException();
            }
        }

        private ItemStack MoveToInventory(int index)
        {
            ItemStack remaining = MainInventory.StoreItemStack(this[index], true);

            if (!remaining.Empty)
                Hotbar.StoreItemStack(remaining, false);

            if (!remaining.Empty)
                MainInventory.StoreItemStack(remaining, false);

            return remaining;
        }

        private ItemStack MoveFromInventory(int index)
        {
            ItemStack remaining = this[index];
            if (remaining.Empty)
                return ItemStack.EmptyStack;

            IItemProvider provider = ItemRepository.GetItemProvider(remaining.ID);

            if (provider is ArmorItem)
                remaining = Armor.StoreItemStack(remaining, false);
            else
                remaining = CraftingGrid.StoreItemStack(remaining, false);

            return remaining;
        }

        protected override void OnWindowChange(WindowChangeEventArgs e)
        {
            // TODO
            throw new NotImplementedException();
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
                            this[slotIndex] = new ItemStack(old.ID, (sbyte)totalItems, old.Metadata, old.Nbt);
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
            // TODO
            throw new NotImplementedException();
        }

        protected override ActionConfirmation HandleRightClick(int slotIndex, IHeldItem heldItem)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
