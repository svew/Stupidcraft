using System;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

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
            IWorld world, GlobalVoxelCoordinates chestLocation,
            GlobalVoxelCoordinates otherHalfLocation, IItemRepository itemRepository);

        IWindowContent NewCraftingBenchWindowContent(ISlots mainInventory, ISlots hotBar,
            ICraftingRepository craftingRepository, IItemRepository itemRepository);
    }
}
