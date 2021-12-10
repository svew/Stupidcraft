using System;
using fNbt;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Networking.Packets;

namespace TrueCraft.Core.Server
{
    // TODO: this should be a server-side only interface.
    public interface IServerSlot : ISlot
    {

        /// <summary>
        /// Gets whether or not this slot has been "saved" by generating
        /// a SetSlot Packet(s) to send to the Player(s).
        /// </summary>
        bool Dirty { get; }

        /// <summary>
        /// Clears the Dirty flag.
        /// </summary>
        void SetClean();

        /// <summary>
        /// Convenience Property for quickly determining the Index of this Slot within its
        /// parent collection of Slots.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Gets a SetSlotPacket for sending the content of this Slot to the Player
        /// </summary>
        /// <param name="windowID">The window ID to use in the SetSlotPacket</param>
        /// <returns>A SetSlotPacket</returns>
        /// <remarks>Calling this method resets the Dirty property.</remarks>
        SetSlotPacket GetSetSlotPacket(sbyte windowID);
    }
}
