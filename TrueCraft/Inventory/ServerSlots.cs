using System;
using System.Collections.Generic;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Server;
using TrueCraft.Core;

namespace TrueCraft.Inventory
{
    public class ServerSlots : Slots<IServerSlot>, IServerSlots
    {
        protected ServerSlots(IItemRepository itemRepository, List<IServerSlot> slots) :
            base(itemRepository, slots)
        {
        }

        public static ServerSlots GetServerSlots(IItemRepository itemRepository, int count)
        {
            List<IServerSlot> slots = new List<IServerSlot>(count);
            for (int j = 0; j < count; j++)
                slots.Add(new ServerSlot(itemRepository, j));

            return new ServerSlots(itemRepository, slots);
        }

        /// <inheritdoc />
        public virtual List<SetSlotPacket> GetSetSlotPackets(sbyte windowID, short baseIndex)
        {
            List<SetSlotPacket> rv = new List<SetSlotPacket>();
            foreach (IServerSlot j in this)
                if (j.Dirty)
                {
                    SetSlotPacket packet = j.GetSetSlotPacket(windowID);
                    packet.SlotIndex += baseIndex;
                    rv.Add(packet);
                }

            return rv;
        }

        public override int Width
        {
            get
            {
                throw new ApplicationException("The server should never call Width.");
            }
        }

        /// <inheritdoc/>
        public override ItemStack StoreItemStack(ItemStack items, bool topUpOnly,
            out List<int> affectedSlotIndices, out List<ItemStack> newItems)
        {
            affectedSlotIndices = new List<int>();
            newItems = new List<ItemStack>();

            if (items.Empty)
                return items;


            // Are there compatible slot(s) that already contain something?
            int j = 0;
            int jul = this.Count;
            ItemStack remaining = items;
            while (j < jul && !remaining.Empty)
            {
                while (j < jul && (this[j].Item.Empty || 0 == this[j].CanAccept(remaining)))
                    j++;
                if (j == jul)
                    break;

                remaining = StoreInSlot(j, remaining);
                affectedSlotIndices.Add(j);
                newItems.Add(this[j].Item);
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

            remaining = StoreInSlot(j, remaining);
            remaining = StoreInSlot(j, remaining);
            affectedSlotIndices.Add(j);
            newItems.Add(this[j].Item);
            return remaining;
        }

    }
}
