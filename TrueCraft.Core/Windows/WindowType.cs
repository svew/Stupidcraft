using System;

namespace TrueCraft.Core.Windows
{
    /// <summary>
    /// An enum to specify types of child windows.
    /// </summary>
    /// <remarks>These values are the ones used in the Type field of the OpenWindowPacket.</remarks>
    public enum WindowType : sbyte
    {
        Inventory = -1,
        Chest = 0,
        CraftingBench = 1,
        Furnace = 2,
        Dispenser = 3
    }
}
