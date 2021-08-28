using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrueCraft.API.Windows
{
    public class WindowChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the index within the WindowContent of the slot being changed.
        /// </summary>
        public int SlotIndex { get; }

        /// <summary>
        /// Gets the value of the ItemStack after the change.
        /// </summary>
        public ItemStack Value { get; }

        public bool Handled { get; set; }

        /// <summary>
        /// Constructs an instance of WindowChangeEventArgs
        /// </summary>
        /// <param name="slotIndex">The index of the Slot within the changed WindowContent instance.</param>
        /// <param name="value">The new ItemStack placed within the Slot.</param>
        public WindowChangeEventArgs(int slotIndex, ItemStack value)
        {
            SlotIndex = slotIndex;
            Value = value;
            Handled = false;
        }

        public override string ToString()
        {
            return $"(SlotIndex: {SlotIndex}; Value={Value}; Handled={Handled})";
        }
    }
}
