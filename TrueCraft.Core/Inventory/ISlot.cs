using System;
using System.ComponentModel;

namespace TrueCraft.Core.Inventory
{
    public interface ISlot : INotifyPropertyChanged
    {
        /// <summary>
        /// Determines how many of the "other" items can be accepted by this Slot.
        /// </summary>
        /// <param name="other">The "other" items which we may want to add to this Slot.</param>
        /// <returns>The number of the "other" items which may be added.  Zero will
        /// be returned if the items are not compatible with the Slot. Items may not be
        /// compatible if the slot already has some items in it, or if it is, for example,
        /// an armor slot.  Zero will be returned if the Slot is already full.</returns>
        int CanAccept(ItemStack other);

        /// <summary>
        /// Gets or sets the ItemStack stored in this inventory Slot.
        /// </summary>
        ItemStack Item { get; set; }
    }
}
