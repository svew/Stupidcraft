using System.Collections.Generic;
using TrueCraft.Core;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Server;

namespace TrueCraft.Inventory
{
    public interface IServerWindow
    {
        /// <summary>
        /// Gets a CloseWindow Packet for this Window.
        /// </summary>
        /// <returns></returns>
        /// <remarks>The server may send such a Packet to the Player to force closure
        /// of the Window.  This may happen, for example, when the block is mined.</remarks>
        CloseWindowPacket GetCloseWindowPacket();

        List<SetSlotPacket> GetDirtySetSlotPackets();
        OpenWindowPacket GetOpenWindowPacket();
        WindowItemsPacket GetWindowItemsPacket();

        /// <summary>
        /// Handles a ClickWindowPacket
        /// </summary>
        /// <param name="client">The client which sent the packet</param>
        /// <param name="packet">The ClickWindowPacket</param>
        void HandleClick(IRemoteClient client, ClickWindowPacket packet);

        void Save();
    }
}