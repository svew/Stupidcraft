using System;
using TrueCraft.API.Logic;
using TrueCraft.API.Server;
using TrueCraft.API.Windows;
using TrueCraft.API.World;
using TrueCraft.Core.Windows;

namespace TrueCraft.Client.Windows
{
    public class WindowContentFactory : IWindowContentFactory
    {
        public WindowContentFactory()
        {
        }

        public IWindowContent NewFurnaceWindowContent(ISlots mainInventory, ISlots hotBar,
            IEventScheduler scheduler, GlobalVoxelCoordinates coordinates,
            IItemRepository itemRepository)
        {
            return new FurnaceWindowContentClient(mainInventory, hotBar, scheduler, coordinates, itemRepository);
        }
    }
}
