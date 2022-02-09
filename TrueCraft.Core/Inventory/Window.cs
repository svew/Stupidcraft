using System;
using System.Linq;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Windows;

namespace TrueCraft.Core.Inventory
{
    /// <summary>
    /// The backing store for the contents of a Window.
    /// Eg. the Inventory Window, or the Crafting Table
    /// </summary>
    public abstract class Window<T> : IWindow<T> where T : ISlot
    {
        private readonly IItemRepository _itemRepository;
        private readonly sbyte _windowID;
        private readonly WindowType _windowType;
        private readonly string _name;
        private readonly ISlots<T>[] _slots;
        private readonly int _count;
        private readonly int _mainInventoryIndex;
        private readonly int _hotbarIndex;

        /// <summary>
        /// Constructs the Window instance
        /// </summary>
        /// <param name="itemRepository"></param>
        /// <param name="windowID">The ID of the Window</param>
        /// <param name="windowType">The Type of the Window</param>
        /// <param name="name">The Name of the Window</param>
        /// <param name="slots">An array of all the Slot collections in the Window.
        /// The MainInventory must be the second last item.
        /// The Hotbar must be the last item.
        /// </param>
        protected Window(IItemRepository itemRepository,sbyte windowID, WindowType windowType, string name, ISlots<T>[] slots)
        {
            _itemRepository = itemRepository;
            _windowID = windowID;
            _windowType = windowType;
            _name = name;
            _slots = slots;
            _count = _slots.Sum(x => x.Count);
            _mainInventoryIndex = _slots.Length - 2;
            _hotbarIndex = _slots.Length - 1;
        }

        protected IItemRepository ItemRepository { get => _itemRepository; }

        public event EventHandler<WindowClosedEventArgs> WindowClosed;

        protected void OnWindowClosed()
        {
            WindowClosed?.Invoke(this, new WindowClosedEventArgs(_windowID));
        }

        public sbyte WindowID { get => _windowID; }

        public string Name { get => _name; }

        public WindowType Type { get => _windowType; }

        public int Count { get => _count; }

        protected ISlots<T>[] Slots { get => _slots; }

        /// <summary>
        /// Gets the index within Slots of the ISlots instance containing the given slotIndex.
        /// </summary>
        /// <param name="slotIndex"></param>
        /// <returns></returns>
        /// <remarks>The given slotIndex is an index within this entire Window.  It is not
        /// an index within the returned ISlots instance.</remarks>
        protected int GetAreaIndex(int slotIndex)
        {
            int rv = 0;
            int count = 0;
            while (count < _count && slotIndex >= count)
            {
                count += _slots[rv].Count;
                if (slotIndex < count)
                    return rv;
                rv++;
            }

            throw new IndexOutOfRangeException($"{nameof(slotIndex)} = {slotIndex} is outside the range of [0,{_count}).");
        }

        /// <inheritdoc />
        public virtual ISlots<T> MainInventory
        {
            get
            {
                return _slots[_mainInventoryIndex];
            }
        }

        /// <inheritdoc />
        public virtual int MainSlotIndex { get; protected set; }

        /// <inheritdoc />
        public virtual ISlots<T> Hotbar
        {
            get
            {
                return _slots[_hotbarIndex];
            }
        }

        /// <inheritdoc />
        public virtual int HotbarSlotIndex { get; protected set; }

        public ItemStack this[int index]
        {
            get
            {
                int startIndex = 0;
                foreach (ISlots<T> area in _slots)
                {
                    if (index >= startIndex && index < startIndex + area.Count)
                        return area[index - startIndex].Item;
                    startIndex += area.Count;
                }
                throw new IndexOutOfRangeException($"{nameof(index)} = {index} is outside the valid range of [0,{_count})");
            }
            set
            {
                int startIndex = 0;
                foreach (var area in _slots)
                {
                    if (index >= startIndex && index < startIndex + area.Count)
                    {
                        area[index - startIndex].Item = value;
                        return;
                    }
                    startIndex += area.Count;
                }
                throw new IndexOutOfRangeException($"{nameof(index)} = {index} is outside the valid range of [0,{_count})");
            }
        }

        /// <inheritdoc />
        public abstract void SetSlots(ItemStack[] slotContents);

        /// <inheritdoc />
        public abstract bool IsOutputSlot(int slotIndex);

        public virtual ItemStack StoreItemStack(ItemStack items)
        {
            ItemStack remaining = Hotbar.StoreItemStack(items, true);
            remaining = MainInventory.StoreItemStack(remaining, true);
            remaining = Hotbar.StoreItemStack(remaining, false);
            return MainInventory.StoreItemStack(remaining, false);
        }
    }
}
