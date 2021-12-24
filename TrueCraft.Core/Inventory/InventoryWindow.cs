using System;
using TrueCraft.Core.Logic;

namespace TrueCraft.Core.Inventory
{
    public abstract class InventoryWindow<T> : Window<T>, IInventoryWindow<T> where T : ISlot
    {
        // NOTE: the values in this enum must match the order in which
        //  the slots collections are added in the constructors.
        public enum AreaIndices
        {
            Crafting = 0,
            Armor = 1,
            Main = 2,
            Hotbar = 3
        }

        private const int _outputSlotIndex = 0;

        public InventoryWindow(IItemRepository itemRepository, ICraftingRepository craftingRepository,
            ISlotFactory<T> slotFactory,
            ISlots<T> mainInventory, ISlots<T> hotBar) :
            base(itemRepository, 0, Windows.WindowType.Inventory, "Inventory",
                GetSlots(itemRepository, craftingRepository, slotFactory, mainInventory, hotBar))
        {
            CraftingOutputSlotIndex = 0;
            ArmorSlotIndex = CraftingOutputSlotIndex + CraftingGrid.Count;
            MainSlotIndex = ArmorSlotIndex + Armor.Count;
            HotbarSlotIndex = MainSlotIndex + MainInventory.Count;
        }

        private static ISlots<T>[] GetSlots(IItemRepository itemRepository,
            ICraftingRepository craftingRepository, ISlotFactory<T> slotFactory,
            ISlots<T> mainInventory, ISlots<T> hotBar)
        {
            ISlots<T>[] rv = new ISlots<T>[4];

            rv[0] = new CraftingArea<T>(itemRepository, craftingRepository, slotFactory, 2, 2);
            rv[1] = new ArmorSlots<T>(itemRepository, slotFactory);
            rv[2] = mainInventory;
            rv[3] = hotBar;

            return rv;
        }

        public ICraftingArea<T> CraftingGrid { get => (ICraftingArea<T>)Slots[(int)AreaIndices.Crafting]; }

        /// <inheritdoc />
        public virtual int CraftingOutputSlotIndex { get; }

        public ISlots<T> Armor { get => Slots[(int)AreaIndices.Armor]; }

        /// <inheritdoc />
        public virtual int ArmorSlotIndex { get; }

        public override bool IsOutputSlot(int slotIndex)
        {
            return slotIndex == _outputSlotIndex;
        }
    }
}
