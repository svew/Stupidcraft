using System;
using TrueCraft.API.Logic;
using TrueCraft.API.Server;
using TrueCraft.API.Windows;
using TrueCraft.API.World;

namespace TrueCraft.Core.Windows
{
    public interface IWindowContentFactory
    {
        IWindowContent NewInventoryWindowContent(ISlots mainInventory, ISlots hotBar,
            ISlots armor, ISlots craftingGrid);

        IWindowContent NewFurnaceWindowContent(ISlots mainInventory, ISlots hotBar,
            IEventScheduler scheduler, GlobalVoxelCoordinates coordinates,
            IItemRepository itemRepository);

        IWindowContent NewChestWindowContent(ISlots mainInventory, ISlots hotBar,
            bool doubleChest, IItemRepository itemRepository);

        // TODO methods for creating other types of Window Content.
    }
}
