using System;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic
{
    public interface IRegisterRecipe
    {
        void RegisterRecipe(ICraftingRecipe recipe);
    }
}
