using System;
using System.Collections.Generic;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Server;

namespace TrueCraft.Inventory
{
    public interface IServerSlots : ISlots<IServerSlot>
    {
        /// <summary>
        /// Gets a SetSlotPacket for each Inventory Slot which has been changed
        /// since the last time SetSlotPackets were sent.
        /// </summary>
        /// <param name="windowID">the Window ID to use in the SetSlotPackets.</param>
        /// <param name="baseIndex">An offset to add to the Index property of the ISlot.</param>
        /// <returns></returns>
        List<SetSlotPacket> GetSetSlotPackets(sbyte windowID, short baseIndex);
    }
}
