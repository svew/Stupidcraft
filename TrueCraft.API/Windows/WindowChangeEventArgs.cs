using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrueCraft.API.Windows
{
    public class WindowChangeEventArgs : EventArgs
    {
        public int SlotIndex { get; }
        public ItemStack Value { get; }
        public bool Handled { get; set; }

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
