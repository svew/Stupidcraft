using System;

namespace TrueCraft.Core.World
{
    /// <summary>
    /// Enumerates the different dimensions in the world in TrueCraft.
    /// </summary>
    public enum DimensionID
    {
        /// <summary>
        /// The Nether dimension.
        /// </summary>
        Nether = -1,

        /// <summary>
        /// The Overworld dimension.
        /// </summary>
        Overworld = 0
    }

    public static class DimensionInfo
    {
        public static string GetName(DimensionID id)
        {
            switch(id)
            {
                case DimensionID.Overworld:
                    return "OverWord";

                case DimensionID.Nether:
                    return "Nether";

                default:
                    return "Default";
            }
        }
    }
}
