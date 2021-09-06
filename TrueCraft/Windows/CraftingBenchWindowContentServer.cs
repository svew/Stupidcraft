﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrueCraft.API.Windows;
using TrueCraft.API.Logic;
using TrueCraft.API;
using TrueCraft.Core.Windows;

namespace TrueCraft.Windows
{
    public class CraftingBenchWindowContentServer : WindowContent, ICraftingBenchWindowContent
    {
        public CraftingBenchWindowContentServer(ISlots mainInventory, ISlots hotBar,
            ICraftingRepository craftingRepository, IItemRepository itemRepository) :
            base(CraftingBenchWindowConstants.Areas(mainInventory, hotBar, craftingRepository),
                itemRepository)
        {
        }

        #region Variables

        private const short CraftingOutputIndex = 0;

        public override string Name
        {
            get
            {
                return "Workbench";
            }
        }

        public override WindowType Type
        {
            get
            {
                return WindowType.CraftingBench;
            }
        }

        public override short[] ReadOnlySlots
        {
            get
            {
                return new[] { CraftingOutputIndex };
            }
        }

        /// <summary>
        /// Clears all the input slots to Empty.
        /// </summary>
        /// <returns>The former content of the input slots.</returns>
        public ItemStack[] ClearInputs()
        {
            ItemStack[] rv = new ItemStack[CraftingGrid.Count - 1];
            for (int j = 0, jul = rv.Length; j < jul; j++)
            {
                rv[j] = CraftingGrid[j + 1];
                CraftingGrid[j + 1] = ItemStack.EmptyStack;
            }

            return rv;
        }

        #region Properties

        public ISlots CraftingGrid { get => SlotAreas[(int)CraftingBenchWindowConstants.AreaIndices.Crafting]; }

        public override ISlots MainInventory { get => SlotAreas[(int)CraftingBenchWindowConstants.AreaIndices.Main]; }

        public override ISlots Hotbar { get => SlotAreas[(int)CraftingBenchWindowConstants.AreaIndices.Hotbar]; }

        public override bool IsPlayerInventorySlot(int slotIndex)
        {
            return slotIndex >= CraftingGrid.Count;
        }

        #endregion

        #endregion

        public override ItemStack[] GetSlots()
        {
            int jul = CraftingGrid.Count;
            ItemStack[] rv = new ItemStack[jul];
            for (int j = 0; j < jul; j++)
                rv[j] = CraftingGrid[j];

            return rv;
        }

        protected override ISlots GetLinkedArea(int index, ItemStack slot)
        {
            if (index < (int)CraftingBenchWindowConstants.AreaIndices.Main)
                return MainInventory;
            return Hotbar;
        }

        public override ItemStack StoreItemStack(ItemStack slot, bool topUpOnly)
        {
            throw new NotImplementedException();
        }

        public override ItemStack MoveItemStack(int index)
        {
            ItemStack remaining = this[index];
            if (remaining.Empty)
                return ItemStack.EmptyStack;

            CraftingBenchWindowConstants.AreaIndices srcAreaIdx = (CraftingBenchWindowConstants.AreaIndices)GetAreaIndex(index);
            if (srcAreaIdx == CraftingBenchWindowConstants.AreaIndices.Crafting)
                return MoveToInventory(index);
            else
                return CraftingGrid.StoreItemStack(remaining, false);
        }

        private ItemStack MoveToInventory(int index)
        {
            ItemStack remaining = MainInventory.StoreItemStack( this[index], true);
            if (remaining.Empty)
                return ItemStack.EmptyStack;

            remaining = Hotbar.StoreItemStack(remaining, false);
            if (remaining.Empty)
                return ItemStack.EmptyStack;

            return MainInventory.StoreItemStack(remaining, false);
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
