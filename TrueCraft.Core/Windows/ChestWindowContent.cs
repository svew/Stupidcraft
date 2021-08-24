using System;
using TrueCraft.API.Windows;
using TrueCraft.Core.Windows;
using TrueCraft.API;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Windows
{
    public class ChestWindowContent : WindowContent
    {
        public ChestWindowContent(ISlots mainInventory, ISlots hotBar, bool doubleChest,
            IItemRepository itemRepository):
            base(
                doubleChest ?
                new[]
                {
                    new Slots(2 * ChestLength, ChestWidth, 2 * ChestHeight), // Chest
                    mainInventory,
                    hotBar
                }:
                new[]
                {
                    new Slots(ChestLength, ChestWidth, ChestHeight), // Chest
                    mainInventory,
                    hotBar
                },
                itemRepository
                )
        {
            DoubleChest = doubleChest;
        }

        private const int ChestAreaIndex = 0;
        private const int MainAreaIndex = 1;
        private const int HotbarAreaIndex = 2;

        public const int ChestIndex = 0;
        private const int ChestWidth = 9;
        private const int ChestHeight = 3;
        public const int ChestLength = ChestWidth * ChestHeight;

        public const int DoubleChestSecondaryIndex = 27;
        private const int _MainIndex = 27;
        private const int _HotbarIndex = 54;
        private const int _DoubleMainIndex = 54;
        private const int _DoubleHotbarIndex = 81;

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
                return SlotAreas[ChestAreaIndex];
            }
        }

        public ISlots MainInventory
        {
            get
            {
                return SlotAreas[MainAreaIndex];
            }
        }

        public int MainIndex
        {
            get
            {
                return DoubleChest ? _DoubleMainIndex : _MainIndex;
            }
        }

        public ISlots Hotbar
        {
            get
            {
                return SlotAreas[HotbarAreaIndex];
            }
        }

        public int HotbarIndex
        {
            get
            {
                return DoubleChest ? _DoubleHotbarIndex : _HotbarIndex;
            }
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
            if (index == ChestAreaIndex)
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
            int srcAreaIdx = GetAreaIndex(index);
            ItemStack remaining = this[index];

            if (srcAreaIdx == ChestAreaIndex)
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
    }
}