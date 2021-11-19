using System;

namespace TrueCraft.Core.Logic
{
    public interface IRegisterBlockProvider
    {
        /// <summary>
        /// Registers a new block provider. This overrides any existing block providers that use the
        /// same block ID.
        /// </summary>
        /// <param name="provider">The Block Provider to be registered</param>
        void RegisterBlockProvider(IBlockProvider provider);
    }
}
