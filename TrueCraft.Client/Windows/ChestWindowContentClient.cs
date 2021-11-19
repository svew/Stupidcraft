using System;
using TrueCraft.Client.Handlers;
using TrueCraft.Client.Modules;
using TrueCraft.Core;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Windows;

namespace TrueCraft.Client.Windows
{
    public class ChestWindowContentClient : WindowContentClient, IChestWindowContent
    {

        public ChestWindowContentClient(ISlots mainInventory, ISlots hotBar, bool doubleChest,
            IItemRepository itemRepository):
            base(ChestWindowConstants.Areas(mainInventory, hotBar, doubleChest),
                itemRepository)
        {
            DoubleChest = doubleChest;

            ChestIndex = 0;
            MainIndex = ChestIndex + ChestInventory.Count;
            HotbarIndex = MainIndex + mainInventory.Count;
        }

        /// <summary>
        /// Gets whether or not this Chest is a double Chest.
        /// </summary>
        public bool DoubleChest { get; }

        public int ChestIndex { get; }
        public int MainIndex { get; }
        public int HotbarIndex { get; }

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

        /// <inheritdoc />
        protected override ActionConfirmation HandleLeftClick(int slotIndex, IHeldItem heldItem)
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
        protected override ActionConfirmation HandleShiftLeftClick(int slotIndex, IHeldItem heldItem)
        {
            ChestWindowConstants.AreaIndices srcArea = (ChestWindowConstants.AreaIndices)GetAreaIndex(slotIndex);

            if (srcArea == ChestWindowConstants.AreaIndices.ChestArea)
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
        protected override ActionConfirmation HandleRightClick(int slotIndex, IHeldItem heldItem)
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
    }
}