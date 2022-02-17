using System;
using System.Collections.Generic;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Server;

namespace TrueCraft.Inventory
{
    public class ServerSlots : Slots<IServerSlot>, IServerSlots
    {
        public ServerSlots(IItemRepository itemRepository, List<IServerSlot> slots) :
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
    }
}
