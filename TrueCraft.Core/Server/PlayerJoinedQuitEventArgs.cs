using System;
using TrueCraft.Core.Networking;

namespace TrueCraft.Core.Server
{
    public class PlayerJoinedQuitEventArgs : EventArgs
    {
        public IRemoteClient Client { get; set; }

        public PlayerJoinedQuitEventArgs(IRemoteClient client)
        {
            Client = client;
        }
    }
}
