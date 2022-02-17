using System;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Inventory
{
    public interface IInventoryFactory<T> where T : ISlot
    {
        public IWindow<T> NewInventoryWindow(IItemRepository itemRepository, ICraftingRepository craftingRepository,
            ISlotFactory<T> slotFactory,
            ISlots<T> mainInventory, ISlots<T> hotBar);

        public ICraftingBenchWindow<T> NewCraftingBenchWindow(IItemRepository itemRepository,
            ICraftingRepository craftingRepository,
            ISlotFactory<T> slotFactory,
            sbyte windowID,
            ISlots<T> mainInventory, ISlots<T> hotBar,
            string name, int width, int height);

        public IChestWindow<T> NewChestWindow(IItemRepository itemRepository,
            ISlotFactory<T> slotFactory,
            sbyte windowID, ISlots<T> mainInventory, ISlots<T> hotBar,
            IWorld world,
            GlobalVoxelCoordinates location, GlobalVoxelCoordinates otherHalf);

        public IFurnaceWindow<T> NewFurnaceWindow(IItemRepository itemRepository, ISlotFactory<T> slotFactory,
            sbyte windowID, IFurnaceSlots furnaceSlots,
            ISlots<T> mainInventory, ISlots<T> hotBar,
            IWorld world, GlobalVoxelCoordinates location);
    }
}
