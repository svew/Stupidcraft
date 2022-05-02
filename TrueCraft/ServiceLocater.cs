using System;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;
using TrueCraft.World;

namespace TrueCraft
{
    public class ServiceLocater : IServiceLocator
    {
        private IWorld? _world;

        private readonly IMultiplayerServer _server;

        private readonly IBlockRepository _blockRepository;

        private readonly IItemRepository _itemRepository;

        /// <summary>
        /// Constructs a new instance of the Service Locator.
        /// </summary>
        public ServiceLocater(IMultiplayerServer server,
            IBlockRepository blockRepository,
            IItemRepository itemRepository)
        {
            if (server is null)
                throw new ArgumentNullException(nameof(server));
            if (blockRepository is null)
                throw new ArgumentNullException(nameof(blockRepository));
            if (itemRepository is null)
                throw new ArgumentNullException(nameof(itemRepository));

            _world = null;
            _server = server;
            _blockRepository = blockRepository;
            _itemRepository = itemRepository;
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

        /// <inheritdoc />
        public IBlockRepository BlockRepository { get => _blockRepository; }

        /// <inheritdoc />
        public IItemRepository ItemRepository { get => _itemRepository; }
    }
}
