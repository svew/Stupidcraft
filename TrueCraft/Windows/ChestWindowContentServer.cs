using System;
using TrueCraft.Core;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Windows;
using TrueCraft.Core.World;

namespace TrueCraft.Windows
{
    public class ChestWindowContentServer : WindowContentServer, IChestWindowContent
    {
        public ChestWindowContentServer(ISlots mainInventory, ISlots hotBar,
            IWorld world,
            GlobalVoxelCoordinates location, GlobalVoxelCoordinates otherHalf,
            IItemRepository itemRepository):
            base(new ISlots[] {
                new ChestSlots(world, location, otherHalf),
                mainInventory, hotBar
                },
                itemRepository)
        {
            DoubleChest = otherHalf is not null;
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
            ChestWindowConstants.AreaIndices srcArea = (ChestWindowConstants.AreaIndices)GetAreaIndex(slotIndex);

            if (srcArea == ChestWindowConstants.AreaIndices.ChestArea)
            {
                ItemStack remaining = Hotbar.StoreItemStack(this[slotIndex], true);
                remaining = MainInventory.StoreItemStack(remaining, true);
                remaining = Hotbar.StoreItemStack(remaining, false);
                remaining = MainInventory.StoreItemStack(remaining, false);
                this[slotIndex] = remaining;

                return true;
            }
            else
            {
                ItemStack remaining = this[slotIndex];
                remaining = ChestInventory.StoreItemStack(remaining, true);
                this[slotIndex] = ChestInventory.StoreItemStack(remaining, false);

                return true;
            }
        }

        protected override bool HandleRightClick(int slotIndex, ref ItemStack itemStaging)
        {
            ItemStack stack = this[slotIndex];
            if (!itemStaging.Empty)
            {
                if (stack.CanMerge(itemStaging))
                {
                    int maxStack = ItemRepository.GetItemProvider(itemStaging.ID).MaximumStack;
                    if (stack.Count < maxStack)
                    {
                        this[slotIndex] = new ItemStack(itemStaging.ID, (sbyte)(stack.Count + 1), itemStaging.Metadata, itemStaging.Nbt);
                        itemStaging = itemStaging.GetReducedStack(1);
                        return true;
                    }
                    else
                    {
                        // Right-click on compatible, but maxed-out stack.
                        // This is a No-Op.  
                        // It is assumed that the server will return accepted=true in
                        // this case (similarly to such cases in the Inventory Window).
                        return true;
                    }
                }
                else
                {
                    // Right-click on an incompatible slot => exchange stacks.
                    this[slotIndex] = itemStaging;
                    itemStaging = stack;
                    return true;
                }
            }
            else
            {
                // Right-clicking an empty hand on an empty slot is a No-Op.
                // It is assumed that the server will return accepted=true in
                // this case (similarly to such cases in the Inventory Window).
                if (stack.Empty)
                    return true;

                int cnt = stack.Count;
                int numToPickUp = cnt / 2 + (cnt & 0x0001);

                itemStaging = new ItemStack(stack.ID, (sbyte)numToPickUp, stack.Metadata, stack.Nbt);
                this[slotIndex] = stack.GetReducedStack(numToPickUp);

                return true;
            }
        }
    }
}