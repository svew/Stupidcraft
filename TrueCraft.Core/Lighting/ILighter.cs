using System;

namespace TrueCraft.Core.Lighting
{
    public interface ILighter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="operation"></param>
        void DoLightingOperation(LightingOperation operation);
    }
}
