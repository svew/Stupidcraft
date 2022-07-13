using System;
using TrueCraft.Core;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;
using TrueCraft.World;

namespace TrueCraft
{
    public class ServerServiceLocator : ServiceLocator, IServerServiceLocator
    {
        private IWorld? _world;

        private readonly IMultiplayerServer _server;

        /// <summary>
        /// Constructs a new instance of the Service Locator.
        /// </summary>
        public ServerServiceLocator(IMultiplayerServer server,
            IServiceLocator serviceLocator) :
            base(serviceLocator.BlockRepository, serviceLocator.ItemRepository)
        {
            if (server is null)
                throw new ArgumentNullException(nameof(server));

            _world = null;
            _server = server;
        }

        /// <inheritdoc />
        public IWorld World
        {
            get
            {
                // It is an error if you are calling get before setting the World.
                if (_world is null)
                    throw new InvalidOperationException($"{nameof(World)} is not yet set");

                return _world;
            }
            set
            {
                if (_world is not null)
                    throw new InvalidOperationException($"{nameof(World)} has already been set.");
                _world = value;
            }
        }

        /// <inheritdoc />
        public IMultiplayerServer Server { get => _server; }
    }
}
