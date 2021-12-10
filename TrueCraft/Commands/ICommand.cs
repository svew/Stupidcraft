using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrueCraft.Core.Networking;
using TrueCraft.Inventory;

namespace TrueCraft.Core.Server
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        string[] Aliases { get; }
        void Handle(IRemoteClient client, string alias, string[] arguments);
        void Help(IRemoteClient client, string alias, string[] arguments);
    }
}
