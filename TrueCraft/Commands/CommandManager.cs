using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Server;

namespace TrueCraft.Commands
{
    public class CommandManager : ICommandManager
    {
        private static CommandManager _instance = null;

        private readonly IList<ICommand> _commands;

        private CommandManager()
        {
            _commands = new List<ICommand>();
            LoadCommands();
        }

        public static ICommandManager Instance
        {
            get
            {
                if (_instance is null)
                    _instance = new CommandManager();
                return _instance;
            }
        }

        private void LoadCommands()
        {
            var truecraftAssembly = Assembly.GetExecutingAssembly();

            var types = truecraftAssembly.GetTypes()
                .Where(t => typeof (ICommand).IsAssignableFrom(t))
                .Where(t => !t.IsDefined(typeof(DoNotAutoLoadAttribute), true))
                .Where(t => !t.IsAbstract);

            foreach (var command in types.Select(type => (ICommand)Activator.CreateInstance(type)))
                _commands.Add(command);
        }

        /// <summary>
        ///     Tries to find the specified command by first performing a
        ///     case-insensitive search on the command names, then a
        ///     case-sensitive search on the aliases.
        /// </summary>
        /// <param name="client">Client which called the command</param>
        /// <param name="alias">Case-insensitive name or case-sensitive alias of the command</param>
        /// <param name="arguments"></param>
        public void HandleCommand(IRemoteClient client, string alias, string[] arguments)
        {
            ICommand foundCommand = FindByName(alias) ?? FindByAlias(alias);
            if (foundCommand == null)
            {
                client.SendMessage("Invalid command \"" + alias + "\".");
                return;
            }
            foundCommand.Handle(client, alias, arguments);
        }

        public ICommand FindByName(string name)
        {
            return _commands.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public ICommand FindByAlias(string alias)
        {
            // uncomment below if alias searching should be case-insensitive
            return _commands.FirstOrDefault(c => c.Aliases.Contains(alias /*, StringComparer.OrdinalIgnoreCase*/));
        }

        #region IList<ICommand>
        public ICommand this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int Count { get => _commands.Count; }

        public bool IsReadOnly { get => true; }

        public int IndexOf(ICommand item)
        {
            return _commands.IndexOf(item);
        }

        public void Insert(int index, ICommand item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public void Add(ICommand item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(ICommand item)
        {
            return _commands.Contains(item);
        }

        public void CopyTo(ICommand[] array, int arrayIndex)
        {
            _commands.CopyTo(array, arrayIndex);
        }

        public bool Remove(ICommand item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<ICommand> GetEnumerator()
        {
            return _commands.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _commands.GetEnumerator();
        }
        #endregion
    }
    public class DoNotAutoLoadAttribute : Attribute
    {
    }
}
