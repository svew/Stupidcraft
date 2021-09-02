using System;
using TrueCraft.API.Logic;
using TrueCraft.API.Server;
using TrueCraft.API.Windows;
using TrueCraft.API.World;

namespace TrueCraft.Core.Windows
{
    public interface IWindowContentFactory
    {
        IWindowContent NewFurnaceWindowContent(ISlots mainInventory, ISlots hotBar,
            IEventScheduler scheduler, GlobalVoxelCoordinates coordinates,
            IItemRepository itemRepository);

        // TODO methods for creating other types of Window Content.
    }
}
