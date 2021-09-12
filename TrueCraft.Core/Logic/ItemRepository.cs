using System;
using TrueCraft.API.Logic;
using System.Collections.Generic;
using System.Linq;
using TrueCraft.API.Entities;
using TrueCraft.API;
using TrueCraft.API.World;

namespace TrueCraft.Core.Logic
{
    public class ItemRepository : IItemRepository
    {
        private readonly List<IItemProvider> ItemProviders;

        private static ItemRepository _singleton;

        private ItemRepository()
        {
            ItemProviders = new List<IItemProvider>();
        }

        public static IItemRepository Get()
        {
            if (object.ReferenceEquals(_singleton, null))
            {
                _singleton = new ItemRepository();
                _singleton.DiscoverItemProviders();
            }

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

        private void DiscoverItemProviders()
        {
            var providerTypes = new List<Type>();
            // TODO: This can only enumerate currently loaded assemblies.
            //  Thus, it will be unable to discover any extensions/mods.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes().Where(t =>
                    typeof(IItemProvider).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    providerTypes.Add(type);
                }
            }

            providerTypes.ForEach(t =>
            {
                var instance = (IItemProvider)Activator.CreateInstance(t);
                RegisterItemProvider(instance);
            });
        }
    }
}