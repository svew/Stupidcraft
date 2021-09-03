using System;
using TrueCraft.API.Windows;
using TrueCraft.API.Logic;
using TrueCraft.API;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Items;
using TrueCraft.Core.Windows;

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

        public override short[] ReadOnlySlots
        {
            get
            {
                return new[] { InventoryWindowConstants.CraftingOutputIndex };
            }
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

        protected override void HandleLeftClick()
        {
            // TODO
            throw new NotImplementedException();
        }

        protected override void HandleShiftLeftClick()
        {
            // TODO
            throw new NotImplementedException();
        }

        protected override void HandleRightClick()
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
