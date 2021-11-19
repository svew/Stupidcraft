using System;
using System.Collections.Generic;

namespace TrueCraft.Core.Logic
{
    public class CraftingRepository : ICraftingRepository, IRegisterRecipe
    {
        private readonly List<ICraftingRecipe> _recipes;

        private static CraftingRepository _singleton;

        private CraftingRepository()
        {
            _recipes = new List<ICraftingRecipe>();
        }

        internal static IRegisterRecipe Init(IDiscover discover)
        {
            if (!object.ReferenceEquals(_singleton, null))
                return _singleton;

            _singleton = new CraftingRepository();
            discover.DiscoverRecipes(_singleton);

            return _singleton;
        }

        public static ICraftingRepository Get()
        {
#if DEBUG
            if (object.ReferenceEquals(_singleton, null))
                throw new ApplicationException("Call to CraftingRepository.Get without initialization.");
#endif

            return _singleton;
        }

        public ICraftingRecipe GetRecipe(CraftingPattern pattern)
        {
            foreach (ICraftingRecipe r in _recipes)
                if (r.Pattern == pattern)
                    return r;

            return null;
        }

        public void RegisterRecipe(ICraftingRecipe recipe)
        {
            _recipes.Add(recipe);
        }
    }
}