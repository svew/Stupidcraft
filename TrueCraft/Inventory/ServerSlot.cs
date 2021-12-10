using System;
using System.Runtime.CompilerServices;
using TrueCraft.Core;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Server;

namespace TrueCraft.Inventory
{
    public class ServerSlot : Slot, IServerSlot
    {
        private bool _dirty = false;

        public ServerSlot(IItemRepository itemRepository) : base(itemRepository)
        {
            Index = -1;
        }

        public ServerSlot(IItemRepository itemRepository, int index) : base(itemRepository)
        {
            Index = index;
        }

        /// <inheritdoc />
        public virtual bool Dirty
        {
            get => _dirty;
            private set
            {
                if (_dirty == value) return;
                _dirty = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc />
        public virtual void SetClean()
        {
            Dirty = false;
        }

        /// <inheritdoc />
        public virtual int Index { get; }

        /// <inheritdoc />
        public virtual SetSlotPacket GetSetSlotPacket(sbyte windowID)
        {
            Dirty = false;
            ItemStack item = Item;
            return new SetSlotPacket(windowID, (short)Index, item.ID, item.Count, item.Metadata);
        }

        protected override void OnPropertyChanged([CallerMemberName] string property = null)
        {
            base.OnPropertyChanged(property);
            if (property != nameof(Dirty))
                Dirty = true;
        }

    }
}
