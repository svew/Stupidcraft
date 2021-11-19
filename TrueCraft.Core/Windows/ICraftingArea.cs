using System;
using TrueCraft.Core.Logic;

namespace TrueCraft.Core.Windows
{
    public interface ICraftingArea : ISlots
    {
        /// <summary>
        /// Gets the Item Stack at location x,y in the input area.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        ItemStack GetItemStack(int x, int y);

        /// <summary>
        /// Gets the current Recipe represented by the items in the Crafting Grid.
        /// </summary>
        ICraftingRecipe Recipe { get; }

        /// <summary>
        /// Takes one Recipe's worth of output from the output slot.
        /// </summary>
        /// <returns>The output of one recipe.</returns>
        public ItemStack TakeOutput();
    }
}
