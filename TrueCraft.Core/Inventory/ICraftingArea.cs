using System;
using TrueCraft.Core.Logic;

namespace TrueCraft.Core.Inventory
{
    public interface ICraftingArea<T> : ISlots<T> where T : ISlot
    {
        /// <summary>
        /// Gets the Height (in slots) of the Crafting Area's inputs.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the Item Stack at location x,y in the input area.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        ItemStack GetItemStack(int x, int y);

        /// <summary>
        /// Gets a 2-dimensional array containing the ItemStacks in the Crafting Area's Input.
        /// </summary>
        /// <returns></returns>
        ItemStack[,] GetItemStacks();

        /// <summary>
        /// Gets the current Recipe represented by the items in the Crafting Grid.
        /// </summary>
        ICraftingRecipe? Recipe { get; }

        /// <summary>
        /// Takes one Recipe's worth of output from the output slot.
        /// </summary>
        /// <returns>The output of one recipe.</returns>
        public ItemStack TakeOutput();
    }
}
