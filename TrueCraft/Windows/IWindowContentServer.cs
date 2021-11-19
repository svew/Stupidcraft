using System;
using TrueCraft.Core;
using TrueCraft.Core.Windows;

namespace TrueCraft.Windows
{
    public interface IWindowContentServer : IWindowContent
    {
        /// <summary>
        /// Server-Side handling of clicks on the specified Slot in the Window.
        /// </summary>
        /// <param name="slotIndex">The index of the Slot within the Window</param>
        /// <param name="right">True if this is a right click</param>
        /// <param name="shift">True if Shift was held down while clicking.</param>
        /// <param name="itemStaging">The ItemStack being moved by the mouse.</param>
        bool HandleClick(int slotIndex, bool right, bool shift, ref ItemStack itemStaging);
    }
}
