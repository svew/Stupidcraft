using System;
using TrueCraft.API.Logic;
using System.Collections.Generic;
using System.Linq;
using TrueCraft.API.Entities;
using TrueCraft.API;
using TrueCraft.API.World;

namespace TrueCraft.Core.Logic
{
    public class ItemRepository : IItemRepository, IRegisterItemProvider
    {
        private readonly List<IItemProvider> ItemProviders;

        private static ItemRepository _singleton;

        private ItemRepository()
        {
            ItemProviders = new List<IItemProvider>();
        }

        internal static IRegisterItemProvider Init(IDiscover discover)
        {
#if DEBUG
            if (!object.ReferenceEquals(_singleton, null))
                throw new ApplicationException("Multiple calls to ItemRepository.Init detected.");
#endif
            _singleton = new ItemRepository();
            discover.DiscoverItemProviders(_singleton);

            return _singleton;
        }

        public static IItemRepository Get()
        {
#if DEBUG
            if (object.ReferenceEquals(_singleton, null))
                throw new ApplicationException("Call to ItemRepository.Get without initialization.");
#endif
            return _singleton;
        }

        public IItemProvider GetItemProvider(short id)
        {
            // TODO: Binary search
            for (int i = 0; i < ItemProviders.Count; i++)
            {
                if (ItemProviders[i].ID == id)
                    return ItemProviders[i];
            }
            return null;
        }

        /// <inheritdoc />
        public void RegisterItemProvider(IItemProvider provider)
        {
            int i;
            for (i = ItemProviders.Count - 1; i >= 0; i--)
            {
                if (provider.ID == ItemProviders[i].ID)
                {
                    ItemProviders[i] = provider; // Override
                    return;
                }
                if (ItemProviders[i].ID < provider.ID)
                    break;
            }
            ItemProviders.Insert(i + 1, provider);
        }
    }
}