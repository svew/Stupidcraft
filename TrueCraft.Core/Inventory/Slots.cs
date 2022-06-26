using System;
using System.Collections;
using System.Collections.Generic;
using TrueCraft.Core;
using TrueCraft.Core.Logic;

namespace TrueCraft.Core.Inventory
{
    public class Slots<T> : ISlots<T> where T : ISlot
    {
        private readonly IItemRepository _itemRepository;
        private readonly List<T> _lst;
        private readonly int _width;

        public Slots(IItemRepository itemRepository, List<T> slots)
        {
            _itemRepository = itemRepository;
            _lst = slots;
            _width = 0;
        }

        public Slots(IItemRepository itemRepository, List<T> slots, int width)
        {
            _itemRepository = itemRepository;
            _lst = slots;
            _width = width;
        }

        /// <inheritdoc />
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
                while (j < jul && this[j].Item.Empty)
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
            while (j < jul && !this[j].Item.Empty)
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
            if (!this[index].Item.CanMerge(items))
                return items;

            int maxStack = _itemRepository.GetItemProvider(items.ID)!.MaximumStack;

            ItemStack curContent = this[index].Item;
            int numToStore = Math.Min(maxStack - curContent.Count, items.Count);
            this[index].Item = new ItemStack(items.ID, (sbyte)(curContent.Count + numToStore), items.Metadata, items.Nbt);
            return (numToStore < items.Count) ?
                new ItemStack(items.ID, (sbyte)(items.Count - numToStore), items.Metadata, items.Nbt) :
                ItemStack.EmptyStack;
        }

        public virtual int Width
        {
            get
            {
                return _width;
            }
        }

        #region IList<T>
        public virtual T this[int index]
        {
            get => _lst[index];
            set => throw new NotSupportedException();
        }

        public int Count => _lst.Count;

        public bool Contains(T item)
        {
            return _lst.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _lst.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator() => _lst.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _lst.GetEnumerator();

        public int IndexOf(T item)
        {
            return _lst.IndexOf(item);
        }

        public bool IsReadOnly => true;
        public void Add(T item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public void Insert(int index, T item) => throw new NotSupportedException();
        public bool Remove(T item) => throw new NotSupportedException();
        public void RemoveAt(int index) => throw new NotSupportedException();
        #endregion
    }
}
