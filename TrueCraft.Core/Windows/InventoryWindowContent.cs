using System;
using TrueCraft.API.Windows;
using TrueCraft.API.Logic;
using TrueCraft.API;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Windows
{
    public class InventoryWindowContent : WindowContent, IInventoryWindowContent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainInventory"></param>
        /// <param name="hotBar"></param>
        /// <param name="armor"></param>
        /// <param name="craftingGrid"></param>
        public InventoryWindowContent(ISlots mainInventory, ISlots hotBar,
            ISlots armor, ISlots craftingGrid) : base(
            new[]
                {
                    craftingGrid,
                    armor,
                    mainInventory,
                    hotBar
                },
            BlockProvider.ItemRepository)
        {
        }

        #region Variables
        private enum AreaIndex
        {
            Crafting = 0,
            Armor = 1,
            Main = 2,
            Hotbar = 3
        }

        public const int InventoryWidth = 9;
        public const int InventoryHeight = 3;
        public const int InventoryLength = InventoryWidth * InventoryHeight;

        public const short HotbarIndex = 36;
        public const short HotbarLength = 9;

        public const short CraftingGridIndex = 1;
        public const short CraftingOutputIndex = 0;
        public const short ArmorIndex = 5;
        public const short MainIndex = 9;

        public static bool IsPlayerInventorySlot(int slotIndex)
        {
            return slotIndex >= MainIndex && slotIndex < MainIndex + InventoryLength;
        }

        public static bool IsHotbarIndex(int slotIndex)
        {
            return slotIndex >= HotbarIndex && slotIndex < HotbarIndex + HotbarLength;
        }

        public static bool IsArmorIndex(int slotIndex)
        {
            return slotIndex >= ArmorIndex && slotIndex < ArmorIndex + 4;   // TODO hard-coded constant
        }

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

        public override short[] ReadOnlySlots
        {
            get
            {
                return new[] { CraftingOutputIndex };
            }
        }

        #region Properties

        public ISlots CraftingGrid { get => SlotAreas[(int)AreaIndex.Crafting]; }

        public ISlots Armor { get => SlotAreas[(int)AreaIndex.Armor]; }

        public ISlots MainInventory { get => SlotAreas[(int)AreaIndex.Main]; }

        public ISlots Hotbar { get => SlotAreas[(int)AreaIndex.Hotbar]; }

        #endregion

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
            AreaIndex src = (AreaIndex)GetAreaIndex(index);

            switch (src)
            {
                case AreaIndex.Crafting:
                case AreaIndex.Armor:
                    return MoveToInventory(index);

                case AreaIndex.Main:
                case AreaIndex.Hotbar:
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
            // TODO restore to abstract & implement client & server versions.
            throw new NotImplementedException();
        }
    }
}
