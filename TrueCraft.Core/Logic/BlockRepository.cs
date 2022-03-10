using System;
using TrueCraft.Core.Entities;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic
{
    public class BlockRepository : IBlockRepository, IBlockPhysicsProvider, IRegisterBlockProvider
    {
        private readonly IBlockProvider[] BlockProviders = new IBlockProvider[0x100];

        private static BlockRepository _singleton = null;

        private BlockRepository()
        {

        }

        internal static IRegisterBlockProvider Init(IDiscover discover)
        {
            // Creating a new Single Player World requires an initialized
            // Block Provider.  Starting an existing world must also initialize
            // the Block Provider.  Thus, we cannot guarantee only one call to
            // this method.  Subsequent calls are ignored.
            if (!(object.ReferenceEquals(_singleton, null)))
                return _singleton;

            _singleton = new BlockRepository();
            discover.DiscoverBlockProviders(_singleton);
            return _singleton;
        }

        /// <summary>
        /// Gets the single instance of the BlockRepository.
        /// </summary>
        /// <returns>The BlockRepository.</returns>
        public static BlockRepository Get()
        {
#if DEBUG
            if (object.ReferenceEquals(_singleton, null))
                throw new ApplicationException("Call to BlockRepository.Get without initialization.");
#endif
            return _singleton;
        }

        public IBlockProvider GetBlockProvider(byte id)
        {
            return BlockProviders[id];
        }

        /// <inheritdoc />
        public void RegisterBlockProvider(IBlockProvider provider)
        {
            BlockProviders[provider.ID] = provider;
        }

        public BoundingBox? GetBoundingBox(IDimension world, GlobalVoxelCoordinates coordinates)
        {
            // TODO: Block-specific bounding boxes
            var id = world.GetBlockID(coordinates);
            if (id == 0) return null;
            var provider = BlockProviders[id];
            return provider.BoundingBox;
        }
    }
}