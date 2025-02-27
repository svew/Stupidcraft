﻿using System;
using System.Diagnostics;
using TrueCraft.Client.Events;
using TrueCraft.Core;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;

namespace TrueCraft.Client.Handlers
{
    internal static class PacketHandlers
    {
        public static void RegisterHandlers(MultiplayerClient client)
        {
            client.RegisterPacketHandler(new HandshakeResponsePacket().ID, HandleHandshake);
            client.RegisterPacketHandler(new ChatMessagePacket().ID, HandleChatMessage);
            client.RegisterPacketHandler(new SetPlayerPositionPacket().ID, HandlePositionAndLook);
            client.RegisterPacketHandler(new LoginResponsePacket().ID, HandleLoginResponse);
            client.RegisterPacketHandler(new UpdateHealthPacket().ID, HandleUpdateHealth);
            client.RegisterPacketHandler(new TimeUpdatePacket().ID, HandleTimeUpdate);

            client.RegisterPacketHandler(new ChunkPreamblePacket().ID, ChunkHandlers.HandleChunkPreamble);
            client.RegisterPacketHandler(new ChunkDataPacket().ID, ChunkHandlers.HandleChunkData);
            client.RegisterPacketHandler(new BlockChangePacket().ID, ChunkHandlers.HandleBlockChange);

            client.RegisterPacketHandler(new WindowItemsPacket().ID, InventoryHandlers.HandleWindowItems);
            client.RegisterPacketHandler(new SetSlotPacket().ID, InventoryHandlers.HandleSetSlot);
            client.RegisterPacketHandler(new CloseWindowPacket().ID, InventoryHandlers.HandleCloseWindowPacket);
            client.RegisterPacketHandler(new OpenWindowPacket().ID, InventoryHandlers.HandleOpenWindowPacket);
            client.RegisterPacketHandler(new UpdateProgressPacket().ID, InventoryHandlers.HandleUpdateProgressPacket);
            client.RegisterPacketHandler(new TransactionStatusPacket().ID, InventoryHandlers.HandleTransactionStatusPacket);
        }

        public static void HandleChatMessage(IPacket _packet, MultiplayerClient client)
        {
            var packet = (ChatMessagePacket)_packet;
            client.OnChatMessage(new ChatMessageEventArgs(packet.Message));
        }

        public static void HandleHandshake(IPacket _packet, MultiplayerClient client)
        {
            var packet = (HandshakeResponsePacket)_packet;
            if (packet.ConnectionHash != "-")
            {
                Console.WriteLine("Online mode is not supported");
                Process.GetCurrentProcess().Kill();
            }
            // TODO: Authentication
            client.QueuePacket(new LoginRequestPacket(PacketReader.Version, client.User.Username));
        }

        public static void HandleLoginResponse(IPacket _packet, MultiplayerClient client)
        {
            var packet = (LoginResponsePacket)_packet;
            client.EntityID = packet.EntityID;
            client.QueuePacket(new PlayerGroundedPacket());
        }

        public static void HandlePositionAndLook(IPacket _packet, MultiplayerClient client)
        {
            var packet = (SetPlayerPositionPacket)_packet;
            // Note: This packet is received once from the server on initial login.
            //     It seems to serve as an acknowledgement of that login.
            //     Setting the client Position will send a PositionAndLook packet
            //     back to the Server, which is required.
            client.Position = new Vector3(packet.X, packet.Y, packet.Z);
            client.LoggedIn = true;
            // TODO: Pitch and yaw
        }

        public static void HandleUpdateHealth(IPacket _packet, MultiplayerClient client)
        {
            var packet = (UpdateHealthPacket)_packet;
            client.Health = packet.Health;
        }

        public static void HandleTimeUpdate(IPacket _packet, MultiplayerClient client)
        {
            TimeUpdatePacket packet = (TimeUpdatePacket)_packet;
            client.Dimension.TimeOfDay = packet.Time % 24000;
        }
    }
}