using System;
using System.Collections.Generic;
using TrueCraft.Core.Logic;

namespace TrueCraft.Core.Inventory
{
    public class SlotFactory<T> : ISlotFactory<T> where T : ISlot
    {
        // NOTE this must be initialized by a call to RegisterSlotFactory
        // prior to using any methods in this class.  Otherwise,
        // a NullReferenceException will occur.
        private static ISlotFactory<T> _impl = null!;

        public static void RegisterSlotFactory(ISlotFactory<T> slotFactory)
        {
            _impl = slotFactory;
        }

        public static ISlotFactory<T> Get()
        {
            return _impl;
        }

        public SlotFactory()
        {
        }

        public T GetSlot(IItemRepository itemRepository)
        {
            return _impl.GetSlot(itemRepository);
        }

        public List<T> GetSlots(IItemRepository itemRepository, int count)
        {
            return _impl.GetSlots(itemRepository, count);
        }
    }
}
