using System;

namespace TrueCraft.Core.Inventory
{
    public class WindowClosedEventArgs : EventArgs
    {
        private readonly sbyte _windowID;

        public WindowClosedEventArgs(sbyte windowID)
        {
            _windowID = windowID;
        }

        /// <summary>
        /// Gets the ID of the Window which was closed.
        /// </summary>
        public sbyte WindowID { get => _windowID; }
    }
}
