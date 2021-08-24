using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrueCraft.API.Windows;
using TrueCraft.API.Logic;
using TrueCraft.API;

namespace TrueCraft.Core.Windows
{
    public class CraftingBenchWindowContent : WindowContent
    {
        public CraftingBenchWindowContent(ISlots mainInventory, ISlots hotBar,
            ICraftingRepository craftingRepository, IItemRepository itemRepository) :
            base(
                new[]
                {
                new CraftingWindowContent(craftingRepository, 3, 3),
                mainInventory,
                hotBar
                },
                itemRepository)
        {
        }

        #region Variables
        private enum AreaIndex
        {
            Crafting = 0,
            Main = 1,
            Hotbar = 2
        }

        public const short HotbarIndex = 37;
        public const short CraftingGridIndex = 1;
        public const short CraftingOutputIndex = 0;
        public const short MainIndex = 10;

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

        public ISlots CraftingGrid { get => SlotAreas[(int)AreaIndex.Crafting]; }

        public ISlots MainInventory { get => SlotAreas[(int)AreaIndex.Main]; }

        public ISlots Hotbar { get => SlotAreas[(int)AreaIndex.Hotbar]; }

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
            if (index < MainIndex)
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

            AreaIndex srcAreaIdx = (AreaIndex)GetAreaIndex(index);
            if (srcAreaIdx == AreaIndex.Crafting)
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
    }
}
