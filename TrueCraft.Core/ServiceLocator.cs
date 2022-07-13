using System;
using TrueCraft.Core.Logic;

namespace TrueCraft.Core
{
    public class ServiceLocator : IServiceLocator
    {
        private readonly IBlockRepository _blockRepository;

        private readonly IItemRepository _itemRepository;

        /// <summary>
        /// Constructs a new instance of the Service Locator.
        /// </summary>
        public ServiceLocator(IBlockRepository blockRepository,
            IItemRepository itemRepository)
        {
            if (blockRepository is null)
                throw new ArgumentNullException(nameof(blockRepository));
            if (itemRepository is null)
                throw new ArgumentNullException(nameof(itemRepository));

            _blockRepository = blockRepository;
            _itemRepository = itemRepository;
        }

        /// <inheritdoc />
        public IBlockRepository BlockRepository { get => _blockRepository; }

        /// <inheritdoc />
        public IItemRepository ItemRepository { get => _itemRepository; }
    }
}
