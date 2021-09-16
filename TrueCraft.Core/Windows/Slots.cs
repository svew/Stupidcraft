using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrueCraft.API.Windows;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Core.Logic;

namespace TrueCraft.Core.Windows
{
    /// <summary>
    /// Represents a collection of related Slots.
    /// </summary>
    /// <remarks>
    /// This collection is used on both the client and server.  Both
    /// must be synchronized.  On the client side, this will be the
    /// backing store for the various different areas of dialogs such
    /// as for inventory, crafting, furnaces, etc.
    /// </remarks>
    public class Slots : ISlots
    {
        public Slots(int length, int width, int height)
        {
#if DEBUG
            if (length < width * height)
                throw new ArgumentException($"{nameof(length)} must be equal to or greater than {nameof(width)} times {nameof(height)}");
#endif
            Count = length;
            _items = new ItemStack[Count];
            Width = width;
            Height = height;
            for (int i = 0; i < _items.Length; i++)
                _items[i] = ItemStack.EmptyStack;
        }

        public int Count { get; }
        public virtual int Width { get; }
        public virtual int Height { get; }

        private ItemStack[] _items;

        public virtual ItemStack this[int index]
        {
            get { return _items[index]; }
            set
            {
                if (value == _items[index])
                    return;

                if (IsValid(value, index))
                    _items[index] = value;
            }
        }

        public virtual ItemStack StoreItemStack(ItemStack items, bool topUpOnly)
        {
            if (items.Empty)
                return items;

            // Are there compatible slot(s) that already contain something?
            int j = 0;
            int jul = this.Count;
            ItemStack remaining = items;
            while (j < jul && !remaining.Empty)
            {
                while (j < jul && this[j].Empty)
                    j++;
                if (j == jul)
                    break;

                remaining = StoreInSlot(j, remaining);
                j++;
            }

            if (topUpOnly || remaining.Empty)
                return remaining;

            // Store any remaining items in the first empty slot.
            j = 0;
            while (j < jul && !this[j].Empty)
                j++;
            if (j == jul)
                return remaining;

            return StoreInSlot(j, remaining);
        }

        /// <summary>
        /// Stores as much as possible of the given items in the specified index.
        /// </summary>
        /// <param name="index">Which slot to store the items in.</param>
        /// <param name="items">The items to store.</param>
        /// <returns>Any items remaining after as many as possible have been stored.</returns>
        private ItemStack StoreInSlot(int index, ItemStack items)
        {
            if (!this[index].CanMerge(items))
                return items;

            int maxStack = ItemRepository.Get().GetItemProvider(items.ID).MaximumStack;

            ItemStack curContent = this[index];
            int numToStore = Math.Min(maxStack - curContent.Count, items.Count);
            this[index] = new ItemStack(items.ID, (sbyte)(curContent.Count + numToStore), items.Metadata, items.Nbt);
            return (numToStore < items.Count) ?
                new ItemStack(items.ID, (sbyte)(items.Count - numToStore), items.Metadata, items.Nbt) :
                ItemStack.EmptyStack;
        }

        /// <summary>
        /// Returns true if the specified slot is valid to
        /// be placed in this index.
        /// </summary>
        protected virtual bool IsValid(ItemStack slot, int index)
        {
            return true;
        }
    }
}
