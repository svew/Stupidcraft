using System;

namespace TrueCraft.Core.Windows
{
    public interface IChestWindowContent : IWindowContent
    {
        /// <summary>
        /// Gets the collection of slots containing the Chest's contents.
        /// </summary>
        ISlots ChestInventory { get; }
    }
}
