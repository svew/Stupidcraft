using System;
using TrueCraft.API.Logic;
using TrueCraft.API.Server;
using TrueCraft.API.Windows;
using TrueCraft.API.World;

namespace TrueCraft.Core.Windows
{
    public class WindowContentFactory : IWindowContentFactory
    {
        private static IWindowContentFactory _impl;

        public static void Init(IWindowContentFactory impl)
        {
            _impl = impl;
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


        public IWindowContent NewChestWindowContent(ISlots mainInventory, ISlots hotBar, bool doubleChest, IItemRepository itemRepository)
        {
#if DEBUG
            if (_impl == null)
                throw new ApplicationException("WindowContentFactory not initialized.");
#endif
            return _impl.NewChestWindowContent(mainInventory, hotBar, doubleChest, itemRepository);
        }
    }
}
