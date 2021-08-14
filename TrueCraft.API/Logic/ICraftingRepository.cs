using TrueCraft.API.Windows;

namespace TrueCraft.API.Logic
{
    public interface ICraftingRepository
    {
        ICraftingRecipe GetRecipe(CraftingPattern craftingArea);
    }
}