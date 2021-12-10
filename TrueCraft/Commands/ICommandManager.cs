using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrueCraft.Core.Networking;
using TrueCraft.Inventory;

namespace TrueCraft.Core.Server
{
    public interface ICommandManager
    {
        IList<ICommand> Commands { get; }

        void HandleCommand(IRemoteClient Client, string Alias, string[] Arguments);
    }
}
