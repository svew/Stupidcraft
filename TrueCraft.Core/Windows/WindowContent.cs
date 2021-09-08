using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrueCraft.API.Windows;
using TrueCraft.API;
using TrueCraft.API.Networking;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Windows
{
    public abstract class WindowContent : IWindowContent, IDisposable, IEventSubject
    {
        protected WindowContent(ISlots[] slotAreas, IItemRepository itemRepository)
        {
            SlotAreas = slotAreas;
            this.ItemRepository = itemRepository;
        }

        protected ISlots[] SlotAreas { get; }

        public IItemRepository ItemRepository { get; }

        public bool IsDisposed { get; private set; }

        public IRemoteClient Client { get; set; }
                
        public sbyte ID { get; set; }
        public abstract string Name { get; }

        public abstract WindowType Type { get; }

        public abstract ISlots MainInventory { get; }

        public abstract ISlots Hotbar { get; }

        public abstract bool IsPlayerInventorySlot(int slotIndex);

        /// <summary>
        /// When shift-clicking items between areas, this method is used
        /// to determine which area links to which.
        /// </summary>
        /// <param name="index">The index of the area the item is coming from</param>
        /// <param name="slot">The item being moved</param>
        /// <returns>The area to place the item into</returns>
        protected abstract ISlots GetLinkedArea(int index, ItemStack slot);

        /// <summary>
        /// Gets the window area to handle this index and adjust index accordingly
        /// </summary>
        /// <param name="index">Input: the slot index within the overall window content.
        /// Output:  the slot index within the "Area".</param>
        /// <returns>The ISlots which contains the input index.</returns>
        protected ISlots GetArea(ref int index)
        {
            int startIndex = 0;
            foreach (var area in SlotAreas)
            {
                if (startIndex <= index && startIndex + area.Count > index)
                {
                    index = index - startIndex;
                    return area;
                }
                startIndex += area.Count;
            }
            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Gets the index of the appropriate area from the WindowAreas array.
        /// </summary>
        /// <param name="index">The index of the slot within the overall window content.</param>
        /// <returns>The index of the "window area" within the window.</returns>
        protected int GetAreaIndex(int index)
        {
            int startIndex = 0;
            for (int i = 0; i < SlotAreas.Length; i++)
            {
                var area = SlotAreas[i];
                if (index >= startIndex && index < startIndex + area.Count)
                    return i;
                startIndex += area.Count;
            }
            throw new IndexOutOfRangeException();
        }

        public virtual int Length
        {
            get 
            {
                return SlotAreas.Sum(a => a.Count);
            }
        }

        public virtual int Length2 { get { return Length; } }

        /// <summary>
        /// Gets a copy of each of the ItemStack instances in this Window Content
        /// </summary>
        /// <returns>An array containing copies of every ItemStack.</returns>
        public virtual ItemStack[] GetSlots()
        {
            int length = SlotAreas.Sum(area => area.Count);
            var slots = new ItemStack[length];
            int startIndex = 0;
            foreach (var windowArea in SlotAreas)
            {
                for (int j = 0, jul = windowArea.Count; j < jul; j++)
                    slots[startIndex + j] = windowArea[j];
                startIndex += windowArea.Count;
            }
            return slots;
        }

        public virtual void SetSlots(ItemStack[] slots)
        {
            int startIndex = 0;
            for (int i = 0, iul = SlotAreas.Length; i < iul; i ++)
            {
                ISlots s = SlotAreas[i];
                int jul = s.Count;
                for (int j = 0; j < jul; j++)
                    s[j] = slots[startIndex + j];
                startIndex += jul;
            }
        }

        public virtual ItemStack this[int index]
        {
            get
            {
                int startIndex = 0;
                foreach (var area in SlotAreas)
                {
                    if (index >= startIndex && index < startIndex + area.Count)
                        return area[index - startIndex];
                    startIndex += area.Count;
                }
                throw new IndexOutOfRangeException($"{nameof(index)} = {index} is outside the valid range of [0,{SlotAreas.Sum((a) => a.Count )})");
            }
            set
            {
                int startIndex = 0;
                foreach (var area in SlotAreas)
                {
                    if (index >= startIndex && index < startIndex + area.Count)
                    {
                        var eventArgs = new WindowChangeEventArgs(index, value);
                        OnWindowChange(eventArgs);
                        if (!eventArgs.Handled)
                            area[index - startIndex] = value;
                        return;
                    }
                    startIndex += area.Count;
                }
                throw new IndexOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public abstract ItemStack StoreItemStack(ItemStack slot, bool topUpOnly);

        /// <summary>
        /// Subclasses must implement this method to handle changes to the Window Content.
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>Subclasses implement this to send (or not) the appropriate packets
        /// to the counterparty.</remarks>
        protected abstract void OnWindowChange(WindowChangeEventArgs e);

        public event EventHandler Disposed;

        public virtual void Dispose()
        {
            if (Disposed != null)
                Disposed(this, null);
            Client = null;
            IsDisposed = true;
        }

        /// <inheritdoc />
        public abstract bool IsOutputSlot(int slotIndex);

        //public static void HandleClickPacket(ClickWindowPacket packet, IWindowContent window, ref ItemStack itemStaging)
        //{
        //    if (packet.SlotIndex >= window.Length || packet.SlotIndex < 0)
        //        return;
        //    var existing = window[packet.SlotIndex];
        //    if (window.ReadOnlySlots.Contains(packet.SlotIndex))
        //    {
        //        if (itemStaging.ID == existing.ID || itemStaging.Empty)
        //        {
        //            if (itemStaging.Empty)
        //                itemStaging = existing;
        //            else
        //                itemStaging.Count += existing.Count;
        //            window[packet.SlotIndex] = ItemStack.EmptyStack;
        //        }
        //        return;
        //    }
        //    if (itemStaging.Empty) // Picking up something
        //    {
        //        if (packet.Shift)
        //        {
        //            window.MoveItemStack(packet.SlotIndex);
        //        }
        //        else
        //        {
        //            if (packet.RightClick)
        //            {
        //                sbyte mod = (sbyte)(existing.Count % 2);
        //                existing.Count /= 2;
        //                itemStaging = existing;
        //                itemStaging.Count += mod;
        //                window[packet.SlotIndex] = existing;
        //            }
        //            else
        //            {
        //                itemStaging = window[packet.SlotIndex];
        //                window[packet.SlotIndex] = ItemStack.EmptyStack;
        //            }
        //        }
        //    }
        //    else // Setting something down
        //    {
        //        if (existing.Empty) // Replace empty slot
        //        {
        //            if (packet.RightClick)
        //            {
        //                var newItem = (ItemStack)itemStaging.Clone();
        //                newItem.Count = 1;
        //                itemStaging.Count--;
        //                window[packet.SlotIndex] = newItem;
        //            }
        //            else
        //            {
        //                window[packet.SlotIndex] = itemStaging;
        //                itemStaging = ItemStack.EmptyStack;
        //            }
        //        }
        //        else
        //        {
        //            if (existing.CanMerge(itemStaging)) // Merge items
        //            {
        //                // TODO: Consider the maximum stack size
        //                if (packet.RightClick)
        //                {
        //                    existing.Count++;
        //                    itemStaging.Count--;
        //                    window[packet.SlotIndex] = existing;
        //                }
        //                else
        //                {
        //                    existing.Count += itemStaging.Count;
        //                    window[packet.SlotIndex] = existing;
        //                    itemStaging = ItemStack.EmptyStack;
        //                }
        //            }
        //            else // Swap items
        //            {
        //                window[packet.SlotIndex] = itemStaging;
        //                itemStaging = existing;
        //            }
        //        }
        //    }
        //}

        /// <inheritdoc />
        public abstract ItemStack MoveItemStack(int index);
    }
}
