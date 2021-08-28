using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrueCraft.API.Windows;
using TrueCraft.API;
using TrueCraft.API.Logic;

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
            Count = length;
            Items = new ItemStack[Count];
            Width = width;
            Height = height;
            for (int i = 0; i < Items.Length; i++)
                Items[i] = ItemStack.EmptyStack;
        }

        public int Count { get; }
        public virtual int Width { get; }
        public virtual int Height { get; }
        public ItemStack[] Items { get; }

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

        public virtual ItemStack StoreItemStack(ItemStack item, bool topUpOnly)
        {
            int j = 0;
            int jul = this.Count;
            ItemStack remaining = item;
            //IItemProvider provider = Server.ItemRepository.GetItemProvider(item.ID);
            int maxStack = 64;   //  TODO: we need access to the ItemRepository to determine this.
            while (j < jul && !remaining.Empty)
            {
                if (this[j].CanMerge(remaining) && this[j].Count < maxStack)
                {
                    sbyte num = (sbyte)Math.Min(remaining.Count, maxStack - this[j].Count);
                    this[j] = new ItemStack(this[j].ID, (sbyte)(this[j].Count + num), this[j].Metadata, this[j].Nbt);
                    remaining = remaining.Count > num ?
                        new ItemStack(remaining.ID, (sbyte)(remaining.Count - num), remaining.Metadata, remaining.Nbt) :
                        ItemStack.EmptyStack;
                }
                j++;
            }

            return remaining;
        }

        /// <summary>
        /// Returns true if the specified slot is valid to
        /// be placed in this index.
        /// </summary>
        protected virtual bool IsValid(ItemStack slot, int index)
        {
            return true;
        }

        protected virtual void OnWindowChange(WindowChangeEventArgs e)
        {
        }
    }
}
