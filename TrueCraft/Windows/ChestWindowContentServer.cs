using System;
using TrueCraft.API.Windows;
using TrueCraft.Core.Windows;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Windows
{
    public class ChestWindowContentServer : WindowContentServer, IChestWindowContent
    {
        public ChestWindowContentServer(ISlots mainInventory, ISlots hotBar, bool doubleChest,
            IItemRepository itemRepository):
            base(ChestWindowConstants.Areas(mainInventory, hotBar, doubleChest),
                itemRepository)
        {
            DoubleChest = doubleChest;
        }

        /// <summary>
        /// Gets whether or not this Chest is a double Chest.
        /// </summary>
        public bool DoubleChest { get; }

        public override string Name
        {
            get
            {
                if (DoubleChest)
                    return "Large Chest";
                return "Chest";
            }
        }

        public override WindowType Type
        {
            get
            {
                return WindowType.Chest;
            }
        }

        public ISlots ChestInventory
        {
            get
            {
                return SlotAreas[(int)ChestWindowConstants.AreaIndices.ChestArea];
            }
        }

        public override ISlots MainInventory
        {
            get
            {
                return SlotAreas[(int)ChestWindowConstants.AreaIndices.MainArea];
            }
        }

        public override ISlots Hotbar
        {
            get
            {
                return SlotAreas[(int)ChestWindowConstants.AreaIndices.HotBarArea];
            }
        }

        public override bool IsPlayerInventorySlot(int slotIndex)
        {
            return slotIndex >= ChestInventory.Count;
        }

        public override int Length2
        {
            get
            {
                return ChestInventory.Count;
            }
        }

        /// <inheritdoc/>
        protected override ISlots GetLinkedArea(int index, ItemStack slot)
        {
            if (index == (int)ChestWindowConstants.AreaIndices.ChestArea)
                return Hotbar;
            else
                return ChestInventory;
        }

        public override ItemStack StoreItemStack(ItemStack slot, bool topUpOnly)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override ItemStack MoveItemStack(int index)
        {
            ChestWindowConstants.AreaIndices srcAreaIdx = (ChestWindowConstants.AreaIndices)GetAreaIndex(index);
            ItemStack remaining = this[index];

            if (srcAreaIdx == ChestWindowConstants.AreaIndices.ChestArea)
                return MoveItemStackToPlayer(remaining);
            else
                return ChestInventory.StoreItemStack(remaining, false);
        }

        private ItemStack MoveItemStackToPlayer(ItemStack src)
        {
            ItemStack remaining = MainInventory.StoreItemStack(src, true);

            if (!remaining.Empty)
                remaining = Hotbar.StoreItemStack(remaining, false);

            if (!remaining.Empty)
                remaining = MainInventory.StoreItemStack(remaining, false);

            return remaining;
        }

        public override bool IsOutputSlot(int slotIndex)
        {
            return false;
        }

        protected override bool HandleLeftClick(int slotIndex, ref ItemStack itemStaging)
        {
            if (itemStaging.Empty)
            {
                itemStaging = this[slotIndex];
                this[slotIndex] = ItemStack.EmptyStack;
                return true;
            }
            else
            {
                if (this[slotIndex].Empty)
                {
                    this[slotIndex] = itemStaging;
                    itemStaging = ItemStack.EmptyStack;
                    return true;
                }

                if (itemStaging.CanMerge(this[slotIndex]))
                {
                    int maxStack = ItemRepository.GetItemProvider(itemStaging.ID).MaximumStack;
                    int numToPlace = Math.Min(maxStack - this[slotIndex].Count, itemStaging.Count);
                    if (numToPlace > 0)
                    {
                        ItemStack slot = this[slotIndex];
                        this[slotIndex] = new ItemStack(slot.ID, (sbyte)(slot.Count + numToPlace), slot.Metadata, slot.Nbt);
                        itemStaging = itemStaging.GetReducedStack(numToPlace);
                    }
                    return true;
                }
                else
                {
                    ItemStack tmp = this[slotIndex];
                    this[slotIndex] = itemStaging;
                    itemStaging = tmp;
                    return true;
                }
            }
        }

        protected override bool HandleShiftLeftClick(int slotIndex, ref ItemStack itemStaging)
        {
            // TODO
            throw new NotImplementedException();
        }

        protected override bool HandleRightClick(int slotIndex, ref ItemStack itemStaging)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}