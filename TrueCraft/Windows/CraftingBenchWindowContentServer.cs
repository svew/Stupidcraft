using System;
using TrueCraft.Core;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Windows;

namespace TrueCraft.Windows
{
    public class CraftingBenchWindowContentServer : WindowContentServer, ICraftingBenchWindowContent
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

        public override bool IsOutputSlot(int slotIndex)
        {
            return slotIndex == CraftingOutputIndex;
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
            ItemStack[] rv = new ItemStack[this.Length];
            int slotIndex = 0;
            for (int j = 0; j < SlotAreas.Length; j ++)
            {
                ISlots area = SlotAreas[j];
                for (int k = 0; k < area.Count; k ++)
                {
                    rv[slotIndex] = area[k];
                    slotIndex++;
                }
            }

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

        protected override bool HandleLeftClick(int slotIndex, ref ItemStack itemStaging)
        {
            // TODO
            throw new NotImplementedException();
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
