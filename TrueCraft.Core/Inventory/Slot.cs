using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TrueCraft.Core.Logic;

namespace TrueCraft.Core.Inventory
{
    public class Slot : ISlot
    {
        private ItemStack _item = ItemStack.EmptyStack;

        protected readonly IItemRepository _itemRepository;

        /// <summary>
        /// Constructs an empty Inventory Slot.
        /// </summary>
        /// <param name="itemRepository">The Item Repository</param>
        /// <param name="index">The index of this Inventory Slot within its Parent collection.</param>
        public Slot(IItemRepository itemRepository)
        {
            if (itemRepository == null)
                    throw new ArgumentNullException(nameof(itemRepository));

            _itemRepository = itemRepository;
        }

        /// <inheritdoc />
        public virtual int CanAccept(ItemStack other)
        {
            if (other.Empty) return 0;

            if (_item.Empty) return other.Count;

            if (_item.CanMerge(other))
            {
                IItemProvider provider = _itemRepository.GetItemProvider(_item.ID);
                int maxStack = provider.MaximumStack;
                return Math.Min(maxStack - _item.Count, other.Count);
            }

            return 0;
        }

        /// <inheritdoc />
        public virtual ItemStack Item
        {
            get => _item;
            set
            {
                if (_item == value) return;
                _item = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
