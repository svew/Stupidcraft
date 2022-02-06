using System;
using System.Collections.Generic;
using TrueCraft.Core.Networking;

namespace TrueCraft.Commands
{
    public interface ICommandManager : IList<ICommand>
    {
        ICommand FindByName(string name);

        ICommand FindByAlias(string alias);

        void HandleCommand(IRemoteClient Client, string Alias, string[] Arguments);
    }
}
