using System;

namespace TrueCraft.API.Windows
{
    public interface IInventoryWindowContent : IWindowContent
    {
        ItemStack PickUpStack(ItemStack items);
    }
}
