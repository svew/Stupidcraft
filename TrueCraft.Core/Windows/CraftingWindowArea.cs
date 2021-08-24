using TrueCraft.API;
using TrueCraft.API.Windows;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Windows
{
    public class CraftingWindowContent : Slots, ICraftingArea
    {
        public static readonly int CraftingOutput = 0;

        private ICraftingRepository _repository;

        public CraftingWindowContent(ICraftingRepository repository, int width, int height)
            : base(width * height + 1, width, height)
        {
            _repository = repository;
            WindowChange += HandleWindowChange;
        }

        private void HandleWindowChange(object sender, WindowChangeEventArgs e)
        {
            if (_repository == null)
                return;

            CraftingPattern inputPattern = CraftingPattern.GetCraftingPattern(this);

            ICraftingRecipe current = _repository.GetRecipe(inputPattern);
            if (e.SlotIndex == CraftingOutput)
            {
                if (e.Value.Empty && current != null) // Item picked up
                {
                    RemoveItemFromOutput(current);
                    current = _repository.GetRecipe(inputPattern);
                }
            }
            if (current == null)
                Items[CraftingOutput] = ItemStack.EmptyStack;
            else
                Items[CraftingOutput] = current.Output;
        }

        private void RemoveItemFromOutput(ICraftingRecipe recipe)
        {
            // Locate area on crafting bench
            int x, y = 0;
            for (x = 0; x < Width; x++)
            {
                bool found = false;
                for (y = 0; y < Height; y++)
                {
                    if (TestRecipe(recipe, x, y))
                    {
                        found = true;
                        break;
                    }
                }
                if (found) break;
            }

            // Remove items
            for (int _x = 0; _x < recipe.Pattern.Width; _x++)
                for (int _y = 0; _y < recipe.Pattern.Height; _y++)
                    Items[(y + _y) * Width + (x + _x) + 1].Count -= recipe.Pattern[_x, _y].Count;
        }


        private bool TestRecipe(ICraftingRecipe recipe, int x, int y)
        {
            if (x + recipe.Pattern.Width > Width || y + recipe.Pattern.Height > Height)
                return false;

            for (int _x = 0; _x < recipe.Pattern.Width; _x++)
            {
                for (int _y = 0; _y < recipe.Pattern.Height; _y++)
                {
                    ItemStack supplied = GetItemStack(x + _x, y + _y);
                    ItemStack required = recipe.Pattern[_x, _y];
                    if (supplied.ID != required.ID || supplied.Count < required.Count ||
                        required.Metadata != supplied.Metadata)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public ItemStack GetItemStack(int x, int y)
        {
            return this[y * Width + x + 1];
        }
    }
}
