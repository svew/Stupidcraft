using System;
using TrueCraft.API.Logic;
using System.Collections.Generic;
using System.Linq;
using TrueCraft.API.Entities;
using TrueCraft.API;
using TrueCraft.API.World;

namespace TrueCraft.Core.Logic
{
    public class BlockRepository : IBlockRepository, IBlockPhysicsProvider
    {
        private readonly IBlockProvider[] BlockProviders = new IBlockProvider[0x100];

        private static BlockRepository _singleton;

        private BlockRepository()
        {

        }

        /// <summary>
        /// Gets the single instance of the BlockRepository.
        /// </summary>
        /// <returns>The BlockRepository.</returns>
        public static BlockRepository Get()
        {
            if (object.ReferenceEquals(_singleton, null))
            {
                _singleton = new BlockRepository();
                _singleton.DiscoverBlockProviders();
            }

            return _singleton;
        }

        public IBlockProvider GetBlockProvider(byte id)
        {
            return BlockProviders[id];
        }

        public void RegisterBlockProvider(IBlockProvider provider)
        {
            BlockProviders[provider.ID] = provider;
        }

        public void DiscoverBlockProviders()
        {
            var providerTypes = new List<Type>();
            // TODO: this can only enumerate loaded assemblies.  It cannot
            //   load unknown assemblies, such as those containing extended (mod) blocks.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes().Where(t =>
                    typeof(IBlockProvider).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    providerTypes.Add(type);
                }
            }

            providerTypes.ForEach(t =>
            {
                var instance = (IBlockProvider)Activator.CreateInstance(t);
                RegisterBlockProvider(instance);
            });
        }
            
        public BoundingBox? GetBoundingBox(IWorld world, GlobalVoxelCoordinates coordinates)
        {
            // TODO: Block-specific bounding boxes
            var id = world.GetBlockID(coordinates);
            if (id == 0) return null;
            var provider = BlockProviders[id];
            return provider.BoundingBox;
        }
    }
}