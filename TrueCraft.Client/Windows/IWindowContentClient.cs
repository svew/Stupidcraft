using System;
using TrueCraft.Core.Windows;
using TrueCraft.Client.Handlers;
using TrueCraft.Client.Modules;

namespace TrueCraft.Client.Windows
{
    public interface IWindowContentClient : IWindowContent
    {
        /// <summary>
        /// Client-Side handling of clicks on the specified Slot in the Window.
        /// </summary>
        /// <param name="slotIndex">The index of the Slot within the Window</param>
        /// <param name="rightClick">True if this is a right click</param>
        /// <param name="shiftClick">True if Shift was held down while clicking.</param>
        /// <param name="heldItem">The items held by the mouse pointer in the window.</param>
        ActionConfirmation HandleClick(int slotIndex, bool rightClick, bool shiftClick, IHeldItem heldItem);
    }
}
