using System;
using TrueCraft.API.Logic;

namespace TrueCraft.API.Windows
{
    public interface ICraftingArea : IWindowArea
    {
        /// <summary>
        /// Gets the Item Stack at location x,y in the input area.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        ItemStack GetItemStack(int x, int y);

    }
}
