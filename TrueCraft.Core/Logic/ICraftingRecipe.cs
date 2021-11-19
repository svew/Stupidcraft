using System;
using TrueCraft.Core;

namespace TrueCraft.Core.Logic
{
    public interface ICraftingRecipe : IEquatable<ICraftingRecipe>
    {
        CraftingPattern Pattern { get; }

        ItemStack Output { get; }
    }
}
