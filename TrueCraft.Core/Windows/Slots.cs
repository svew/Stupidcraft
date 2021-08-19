using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrueCraft.API.Windows;
using TrueCraft.API;

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
        public Slots(int startIndex, int length, int width, int height)
        {
            StartIndex = startIndex;
            Count = length;
            Items = new ItemStack[Count];
            Width = width;
            Height = height;
            for (int i = 0; i < Items.Length; i++)
                Items[i] = ItemStack.EmptyStack;
        }

        public int StartIndex { get; }
        public int Count { get; }
        public virtual int Width { get; }
        public virtual int Height { get; }
        public ItemStack[] Items { get; }
        public event EventHandler<WindowChangeEventArgs> WindowChange;

        public virtual ItemStack this[int index]
        {
            get { return Items[index]; }
            set
            {
                if (value == Items[index])
                    return;

                if (IsValid(value, index))
                {
                    Items[index] = value;
                    OnWindowChange(new WindowChangeEventArgs(index, value));
                }
            }
        }

        public virtual int MoveOrMergeItem(int index, ItemStack item, ISlots from)
        {
            int emptyIndex = -1;
            //var maximumStackSize = Item.GetMaximumStackSize(new ItemDescriptor(item.Id, item.Metadata));
            // TODO
            var maximumStackSize = 64;
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Empty && emptyIndex == -1)
                    emptyIndex = i;
                else if (this[i].ID == item.ID &&
                    this[i].Metadata == item.Metadata &&
                    this[i].Count < maximumStackSize)
                {
                    // Merging takes precedence over empty slots
                    emptyIndex = -1;
                    if (from != null)
                        from[index] = ItemStack.EmptyStack;
                    if (this[i].Count + item.Count > maximumStackSize)
                    {
                        item = new ItemStack(item.ID, (sbyte)(item.Count - (maximumStackSize - this[i].Count)),
                            item.Metadata, item.Nbt);
                        this[i] = new ItemStack(item.ID, (sbyte)maximumStackSize, item.Metadata, item.Nbt);
                        continue;
                    }
                    this[i] = new ItemStack(item.ID, (sbyte)(this[i].Count + item.Count), item.Metadata);
                    return i;
                }
            }
            if (emptyIndex != -1)
            {
                if (from != null)
                    from[index] = ItemStack.EmptyStack;
                this[emptyIndex] = item;
            }
            return emptyIndex;
        }

        /// <summary>
        /// Returns true if the specified slot is valid to
        /// be placed in this index.
        /// </summary>
        protected virtual bool IsValid(ItemStack slot, int index)
        {
            return true;
        }

        public void CopyTo(ISlots area)
        {
            for (int i = 0; i < area.Count && i < Count; i++)
                area[i] = this[i];
        }

        protected internal virtual void OnWindowChange(WindowChangeEventArgs e)
        {
            if (WindowChange != null)
                WindowChange(this, e);
        }

        public virtual void Dispose()
        {
            WindowChange = null;
        }
    }
}
