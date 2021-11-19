using System;

namespace TrueCraft.Core.Logic
{
    public interface IRegisterItemProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemProvider"></param>
        void RegisterItemProvider(IItemProvider itemProvider);
    }
}
