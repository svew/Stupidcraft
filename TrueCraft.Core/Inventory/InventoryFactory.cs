using System;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Inventory
{
    /// <summary>
    /// 
    /// </summary>
    public class InventoryFactory<T> : IInventoryFactory<T> where T : ISlot
    {
        // NOTE this must be initialized by a call to RegisterInventoryFactory
        // prior to using any methods in this class.  Otherwise,
        // a NullReferenceException will occur.
        private static IInventoryFactory<T> _impl = null!;

        public static void RegisterInventoryFactory(IInventoryFactory<T> factory)
        {
            _impl = factory;
        }

        public IWindow<T> NewInventoryWindow(IItemRepository itemRepository,
            ICraftingRepository craftingRepository,
            ISlotFactory<T> slotFactory,
            ISlots<T> mainInventory, ISlots<T> hotBar)
        {
            return _impl.NewInventoryWindow(itemRepository, craftingRepository,
                slotFactory, mainInventory, hotBar);
        }


        public ICraftingBenchWindow<T> NewCraftingBenchWindow(IItemRepository itemRepository,
            ICraftingRepository craftingRepository,
            ISlotFactory<T> slotFactory,
            sbyte windowID,
            ISlots<T> mainInventory, ISlots<T> hotBar,
            string name, int width, int height)
        {
            return _impl.NewCraftingBenchWindow(itemRepository, craftingRepository,
                slotFactory, windowID, mainInventory, hotBar, name, width, height);
        }

        public IChestWindow<T> NewChestWindow(IItemRepository itemRepository,
            ISlotFactory<T> slotFactory,
            sbyte windowID, ISlots<T> mainInventory, ISlots<T> hotBar,
            IDimension dimension,
            GlobalVoxelCoordinates location, GlobalVoxelCoordinates? otherHalf)
        {
            return _impl.NewChestWindow(itemRepository, slotFactory, windowID,
                mainInventory, hotBar, dimension, location, otherHalf);
        }

        public IFurnaceWindow<T> NewFurnaceWindow(IItemRepository itemRepository, ISlotFactory<T> slotFactory,
            sbyte windowID, IFurnaceSlots furnaceSlots,
            ISlots<T> mainInventory, ISlots<T> hotBar,
            IDimension dimension, GlobalVoxelCoordinates location)
        {
            return _impl.NewFurnaceWindow(itemRepository, slotFactory, windowID,
                furnaceSlots, mainInventory, hotBar, dimension, location);
        }
    }
}
