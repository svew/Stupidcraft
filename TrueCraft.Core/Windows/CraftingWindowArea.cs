using System;
using TrueCraft.Core.Logic;

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
        }

        public override ItemStack this[int index]
        {
            get => base[index];
            set
            {
                base[index] = value;

                if (index == 0)
                    return;

                UpdateOutput();
            }
        }

        private void UpdateOutput()
        {
            CraftingPattern pattern = CraftingPattern.GetCraftingPattern(this);
            this.Recipe = _repository.GetRecipe(pattern);
            base[0] = this.Recipe?.Output ?? ItemStack.EmptyStack;
        }

        /// <inheritdoc />
        public ICraftingRecipe Recipe { get; private set; }

        /// <inheritdoc />
        public ItemStack TakeOutput()
        {
            ItemStack rv = Recipe?.Output ?? ItemStack.EmptyStack;
            if (rv.Empty)
                return rv;

            base[0] = base[0].GetReducedStack(rv.Count);
            RemoveItemsFromInput();
            UpdateOutput();

            return rv;
        }

        private void RemoveItemsFromInput()
        {
            ICraftingRecipe recipe = Recipe;

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
                {
                    int idx = (y + _y) * Width + (x + _x) + 1;
                    base[idx] = base[idx].GetReducedStack(recipe.Pattern[_x, _y].Count);
                }
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
