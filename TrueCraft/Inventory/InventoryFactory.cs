using System;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Inventory
{
    public class InventoryFactory : IInventoryFactory<IServerSlot>
    {
        public InventoryFactory()
        {
        }

        public IWindow<IServerSlot> NewInventoryWindow(IItemRepository itemRepository,
            ICraftingRepository craftingRepository, ISlotFactory<IServerSlot> slotFactory,
            ISlots<IServerSlot> mainInventory, ISlots<IServerSlot> hotBar)
        {
            throw new NotImplementedException();
        }

        public ICraftingBenchWindow<IServerSlot> NewCraftingBenchWindow(IItemRepository itemRepository,
            ICraftingRepository craftingRepository, ISlotFactory<IServerSlot> slotFactory,
            sbyte windowID, ISlots<IServerSlot> mainInventory, ISlots<IServerSlot> hotBar,
            string name, int width, int height)
        {
            throw new NotImplementedException();
        }

        public IChestWindow<IServerSlot> NewChestWindow(IItemRepository itemRepository,
            ISlotFactory<IServerSlot> slotFactory, sbyte windowID, ISlots<IServerSlot> mainInventory,
            ISlots<IServerSlot> hotBar, IWorld world, GlobalVoxelCoordinates location,
            GlobalVoxelCoordinates otherHalf)
        {
            throw new NotImplementedException();
        }

        public IFurnaceWindow<IServerSlot> NewFurnaceWindow(IItemRepository itemRepository,
            ISlotFactory<IServerSlot> slotFactory, sbyte windowID, ISlots<IServerSlot> mainInventory,
            ISlots<IServerSlot> hotBar, IWorld world, GlobalVoxelCoordinates location)
        {
            throw new NotImplementedException();
        }
    }
}
