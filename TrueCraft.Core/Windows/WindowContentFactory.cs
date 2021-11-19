using System;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Windows
{
    public class WindowContentFactory : IWindowContentFactory
    {
        private static IWindowContentFactory _impl;

        public static void Init(IWindowContentFactory impl)
        {
            _impl = impl;
        }

        public IWindowContent NewInventoryWindowContent(ISlots mainInventory, ISlots hotBar,
            ISlots armor, ISlots craftingGrid)
        {
#if DEBUG
            if (_impl == null)
                throw new ApplicationException("WindowContentFactory not initialized.");
#endif
            return _impl.NewInventoryWindowContent(mainInventory, hotBar, armor, craftingGrid);
        }

        public IWindowContent NewFurnaceWindowContent(ISlots mainInventory, ISlots hotBar,
            IEventScheduler scheduler, GlobalVoxelCoordinates coordinates,
            IItemRepository itemRepository)
        {
#if DEBUG
            if (_impl == null)
                throw new ApplicationException("WindowContentFactory not initialized.");
#endif
            return _impl.NewFurnaceWindowContent(mainInventory, hotBar, scheduler, coordinates, itemRepository);
        }


        public IWindowContent NewChestWindowContent(ISlots mainInventory, ISlots hotBar,
            IWorld world, GlobalVoxelCoordinates chestLocation,
            GlobalVoxelCoordinates otherHalfLocation, IItemRepository itemRepository)
        {
#if DEBUG
            if (_impl == null)
                throw new ApplicationException("WindowContentFactory not initialized.");
#endif
            return _impl.NewChestWindowContent(mainInventory, hotBar, world, chestLocation,
                otherHalfLocation, itemRepository);
        }

        public IWindowContent NewCraftingBenchWindowContent(ISlots mainInventory, ISlots hotBar,
            ICraftingRepository craftingRepository, IItemRepository itemRepository)
        {
#if DEBUG
            if (_impl == null)
                throw new ApplicationException("WindowContentFactory not initialized.");
#endif
            return _impl.NewCraftingBenchWindowContent(mainInventory, hotBar, craftingRepository, itemRepository);
        }
    }
}
