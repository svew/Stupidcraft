using System;

namespace TrueCraft.Core.Windows
{
    public interface IInventoryWindowContent : IWindowContent
    {
        ItemStack PickUpStack(ItemStack items);
    }
}
