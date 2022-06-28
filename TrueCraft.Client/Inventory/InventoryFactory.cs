using System;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Client.Inventory
{
    public class InventoryFactory : IInventoryFactory<ISlot>
    {
        public IWindow<ISlot> NewInventoryWindow(IItemRepository itemRepository,
            ICraftingRepository craftingRepository, ISlotFactory<ISlot> slotFactory,
            ISlots<ISlot> mainInventory, ISlots<ISlot> hotBar)
        {
            return new InventoryWindow(itemRepository, craftingRepository, slotFactory, mainInventory, hotBar);
        }

        public IChestWindow<ISlot> NewChestWindow(IItemRepository itemRepository,
            ISlotFactory<ISlot> slotFactory, sbyte windowID,
            ISlots<ISlot> mainInventory, ISlots<ISlot> hotBar,
            IDimension dimension, GlobalVoxelCoordinates location, GlobalVoxelCoordinates? otherHalf)
        {
            return new ChestWindow(itemRepository, slotFactory, windowID, mainInventory, hotBar,
                otherHalf is not null);
        }

        public ICraftingBenchWindow<ISlot> NewCraftingBenchWindow(IItemRepository itemRepository,
            ICraftingRepository craftingRepository, ISlotFactory<ISlot> slotFactory,
            sbyte windowID, ISlots<ISlot> mainInventory, ISlots<ISlot> hotBar,
            string name, int width, int height)
        {
            return new CraftingBenchWindow(itemRepository, craftingRepository, slotFactory, windowID,
                mainInventory, hotBar, name, width, height);
        }

        public IFurnaceWindow<ISlot> NewFurnaceWindow(IItemRepository itemRepository,
            ISlotFactory<ISlot> slotFactory, sbyte windowID, 
            IFurnaceSlots furnaceSlots, ISlots<ISlot> mainInventory, ISlots<ISlot> hotBar,
            IDimension dimension, GlobalVoxelCoordinates location)
        {
            return new FurnaceWindow(itemRepository, slotFactory, windowID,
                mainInventory, hotBar);
        }
    }
}
