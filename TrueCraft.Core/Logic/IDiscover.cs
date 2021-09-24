using System;
namespace TrueCraft.Core.Logic
{
    public interface IDiscover
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="repository"></param>
        void DiscoverItemProviders(IRegisterItemProvider repository);

        /// <summary>
        /// Finds all the available Block Providers and registers them in the Block Repository.
        /// </summary>
        /// <param name="repository"></param>
        void DiscoverBlockProviders(IRegisterBlockProvider repository);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repository"></param>
        void DiscoverRecipes(IRegisterRecipe repository);
    }
}
