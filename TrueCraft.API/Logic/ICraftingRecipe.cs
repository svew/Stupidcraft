using System;
using TrueCraft.API;

namespace TrueCraft.API.Logic
{
    public interface ICraftingRecipe : IEquatable<ICraftingRecipe>
    {
        CraftingPattern Pattern { get; }

        ItemStack Output { get; }
    }
}