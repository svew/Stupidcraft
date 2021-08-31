using System;
using TrueCraft.API.Logic;
using TrueCraft.API.Windows;
using TrueCraft.Core.Windows;

namespace TrueCraft.Client.Windows
{
    public abstract class WindowContentClient : WindowContent
    {
        public WindowContentClient(ISlots[] slotAreas, IItemRepository itemRepository) :
            base(slotAreas, itemRepository)
        {
        }
    }
}
