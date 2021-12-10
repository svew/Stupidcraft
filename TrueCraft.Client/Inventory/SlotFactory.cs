using System;
using System.Collections.Generic;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;

namespace TrueCraft.Client.Inventory
{
    public class SlotFactory : ISlotFactory<ISlot>
    {
        public SlotFactory()
        {
        }

        public ISlot GetSlot(IItemRepository itemRepository)
        {
            return new Slot(itemRepository);
        }

        public List<ISlot> GetSlots(IItemRepository itemRepository, int count)
        {
            List<ISlot> rv = new List<ISlot>(count);
            for (int j = 0; j < count; j++)
                rv.Add(new Slot(itemRepository));

            return rv;
        }
    }
}
