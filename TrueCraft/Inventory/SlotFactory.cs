using System;
using System.Collections.Generic;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;

namespace TrueCraft.Inventory
{
    public class SlotFactory : ISlotFactory<IServerSlot>
    {
        public SlotFactory()
        {
        }

        public IServerSlot GetSlot(IItemRepository itemRepository)
        {
            return new ServerSlot(itemRepository);
        }

        public List<IServerSlot> GetSlots(IItemRepository itemRepository, int count)
        {
            List<IServerSlot> rv = new List<IServerSlot>(count);
            for (int j = 0; j < count; j++)
                rv.Add(new ServerSlot(itemRepository, j));

            return rv;
        }
    }
}
