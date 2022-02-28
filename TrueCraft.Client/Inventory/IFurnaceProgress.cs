using System;

namespace TrueCraft.Client.Inventory
{
    public interface IFurnaceProgress
    {
        /// <summary>
        /// Gets or sets the progress of the current smelting operation
        /// </summary>
        int SmeltingProgress { get; set; }

        /// <summary>
        /// Gets or sets the height of the flame in the Furnace, which indicates
        /// the fraction of the original burn time remaining.
        /// </summary>
        int BurnProgress { get; set; }
    }
}
