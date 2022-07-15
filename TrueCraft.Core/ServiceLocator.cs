using System;
using TrueCraft.Core.Logic;

namespace TrueCraft.Core
{
    public class ServiceLocator : IServiceLocator
    {
        private readonly IBlockRepository _blockRepository;

        private readonly IItemRepository _itemRepository;

        private readonly ICraftingRepository _craftingRepository;

        /// <summary>
        /// Constructs a new instance of the Service Locator.
        /// </summary>
        public ServiceLocator(IBlockRepository blockRepository,
            IItemRepository itemRepository, ICraftingRepository craftingRepository)
        {
            if (blockRepository is null)
                throw new ArgumentNullException(nameof(blockRepository));
            if (itemRepository is null)
                throw new ArgumentNullException(nameof(itemRepository));
            if (craftingRepository is null)
                throw new ArgumentNullException(nameof(craftingRepository));

            _blockRepository = blockRepository;
            _itemRepository = itemRepository;
            _craftingRepository = craftingRepository;
        }

        /// <inheritdoc />
        public IBlockRepository BlockRepository { get => _blockRepository; }

        /// <inheritdoc />
        public IItemRepository ItemRepository { get => _itemRepository; }

        /// <inheritdoc />
        public ICraftingRepository CraftingRepository { get => _craftingRepository; }
    }
}
