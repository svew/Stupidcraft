using System;
using System.Collections.Generic;
using TrueCraft.Core.Logic;

namespace TrueCraft.Core.Inventory
{
    public interface ISlotFactory<T> where T : ISlot
    {
        /// <summary>
        /// Gets an instance of ISlot.
        /// </summary>
        /// <returns></returns>
        T GetSlot(IItemRepository itemRepository);

        /// <summary>
        /// Gets a List containing the specified number of ISlot implementations.
        /// </summary>
        /// <param name="itemRepository"></param>
        /// <param name="count">The number of ISlot instances to return.</param>
        /// <returns></returns>
        List<T> GetSlots(IItemRepository itemRepository, int count);
    }
}
