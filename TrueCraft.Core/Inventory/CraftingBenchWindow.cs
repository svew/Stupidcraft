using System;
using TrueCraft.Core.Logic;

namespace TrueCraft.Core.Inventory
{
    public abstract class CraftingBenchWindow<T> : Window<T>, ICraftingBenchWindow<T> where T : ISlot
    {
        // NOTE: These values must match the order in which the Slot collections
        //  are added in the constructors.
        public enum AreaIndices
        {
            Crafting = 0,
            Main = 1,
            Hotbar = 2
        }

        public CraftingBenchWindow(IItemRepository itemRepository,
            ICraftingRepository craftingRepository,
            ISlotFactory<T> slotFactory,
            sbyte windowID,
            ISlots<T> mainInventory, ISlots<T> hotBar,
            string name, int width, int height) :
            base(itemRepository, windowID, Windows.WindowType.CraftingBench, name,
                new ISlots<T>[] { GetSlots(itemRepository, craftingRepository, slotFactory, width, height), mainInventory, hotBar })
        {
            CraftingOutputSlotIndex = 0;
            MainSlotIndex = CraftingOutputSlotIndex + CraftingArea.Count;
            HotbarSlotIndex = MainSlotIndex + MainInventory.Count;
        }

        private static ISlots<T> GetSlots(IItemRepository itemRepository,
            ICraftingRepository craftingRepository,
            ISlotFactory<T> slotFactory,
            int width, int height)
        {
            return new CraftingArea<T>(itemRepository, craftingRepository, slotFactory, width, height);
        }

        public override bool IsOutputSlot(int slotIndex)
        {
            return slotIndex == 0;
        }

        public ICraftingArea<T> CraftingArea
        {
            get
            {
                return (ICraftingArea<T>)Slots[(int)AreaIndices.Crafting];
            }
        }

        /// <inheritdoc />
        public virtual int CraftingOutputSlotIndex { get; }
    }
}
