using System;
using TrueCraft.Core.Networking;
using System.Net;
using System.Collections.Generic;
using TrueCraft.Core.World;
using TrueCraft.Core.Logging;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Inventory;

namespace TrueCraft.Core.Server
{
    /// <summary>
    /// Called when the given packet comes in from a remote client. Return false to cease communication
    /// with that client.
    /// </summary>
    public delegate void PacketHandler(IPacket packet, IRemoteClient client, IMultiplayerServer server);

    // TODO: this looks like it should be a server-side only concern.
    //        However, moving it is non-trivial due to the large number of
    //        references to it in Core.
    public interface IMultiplayerServer
    {
        event EventHandler<ChatMessageEventArgs> ChatMessageReceived;
        event EventHandler<PlayerJoinedQuitEventArgs> PlayerJoined;
        event EventHandler<PlayerJoinedQuitEventArgs> PlayerQuit;

        IAccessConfiguration AccessConfiguration { get; }
        IPacketReader PacketReader { get; }
        IList<IRemoteClient> Clients { get; }

        // TODO: this returns an IWorld interface.
        /// <summary>
        /// 
        /// </summary>
        object? World { get; }

        IEventScheduler Scheduler { get; }

        [Obsolete()]
        IBlockRepository BlockRepository { get; }

        IItemRepository ItemRepository { get; }
        IPEndPoint? EndPoint { get; }
        bool BlockUpdatesEnabled { get; set; }
        bool EnableClientLogging { get; set; }

        void Start(IPEndPoint endPoint);
        void Stop();
        void RegisterPacketHandler(byte packetId, PacketHandler handler);

        void AddLogProvider(ILogProvider provider);
        void Log(LogCategory category, string text, params object[] parameters);

        void SendMessage(string message, params object[] parameters);

        void DisconnectClient(IRemoteClient client);

        bool PlayerIsWhitelisted(string client);
        bool PlayerIsBlacklisted(string client);
        bool PlayerIsOp(string client);
    }
}
