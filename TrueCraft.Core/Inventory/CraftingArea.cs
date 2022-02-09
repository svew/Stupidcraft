using System;
using System.Collections.Generic;
using System.ComponentModel;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Logic;

namespace TrueCraft.Core.Inventory
{
    public class CraftingArea<T> : Slots<T>, ICraftingArea<T> where T : ISlot
    {
        public static readonly int CraftingOutput = 0;

        private ICraftingRepository _repository;

        public CraftingArea(IItemRepository itemRepository, ICraftingRepository repository,
            ISlotFactory<T> slotFactory,
            int width, int height)
            : base(itemRepository, slotFactory.GetSlots(itemRepository, width * height + 1), width)
        {
            _repository = repository;
            Height = height;

            for (int j = 1, jul = width * height; j <= jul; j++)
                this[j].PropertyChanged += HandleSlotPropertyChanged;
        }

        private void HandleSlotPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateOutput();
        }

        /// <inheritdoc />
        public int Height { get; }

        private void UpdateOutput()
        {
            CraftingPattern pattern = CraftingPattern.GetCraftingPattern(this.GetItemStacks());
            this.Recipe = _repository.GetRecipe(pattern);
            base[0].Item = this.Recipe?.Output ?? ItemStack.EmptyStack;
        }

        /// <inheritdoc />
        public ICraftingRecipe Recipe { get; private set; }

        /// <inheritdoc />
        public ItemStack TakeOutput()
        {
            ItemStack rv = Recipe?.Output ?? ItemStack.EmptyStack;
            if (rv.Empty)
                return rv;

            base[0].Item = base[0].Item.GetReducedStack(rv.Count);
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
                    base[idx].Item = base[idx].Item.GetReducedStack(recipe.Pattern[_x, _y].Count);
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
            return this[y * Width + x + 1].Item;
        }

        public ItemStack[,] GetItemStacks()
        {
            ItemStack[,] rv = new ItemStack[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    rv[x, y] = GetItemStack(x, y);

            return rv;
        }
    }
}
