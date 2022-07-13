using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace TrueCraft.Core.Logic
{
    public class Discover : IDiscover
    {
        public Discover()
        {
        }

        public static IServiceLocator DoDiscovery(IDiscover discoverer)
        {
            IBlockRepository blockRepository = BlockRepository.Init(discoverer);
            IItemRepository itemRepository = ItemRepository.Init(discoverer);

            // TODO: Add Crafting Repository to ServiceLocator
            CraftingRepository.Init(discoverer);

            return new ServiceLocator(blockRepository, itemRepository);
        }

        public virtual void DiscoverBlockProviders(IRegisterBlockProvider repository)
        {
            var providerTypes = new List<Type>();
            Assembly thisAssembly = this.GetType().Assembly;
            foreach (var type in thisAssembly.GetTypes().Where(t =>
                typeof(IBlockProvider).IsAssignableFrom(t) && !t.IsAbstract))
            {
                providerTypes.Add(type);
            }

            providerTypes.ForEach(t =>
            {
                IBlockProvider? instance = (IBlockProvider?)Activator.CreateInstance(t);
                // TODO: If instance is null, it means the developer forgot to
                //       include a parameterless constructor.  Log a warning.
                if (instance is not null)
                    repository.RegisterBlockProvider(instance);
            });
        }

        public virtual void DiscoverItemProviders(IRegisterItemProvider repository)
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
                IItemProvider? instance = (IItemProvider?)Activator.CreateInstance(t);
                // TODO: If instance is null, it means the developer forgot to
                //       include a parameterless constructor.  Log a warning.
                if (instance is not null)
                    repository.RegisterItemProvider(instance);
            });
        }


        public virtual void DiscoverRecipes(IRegisterRecipe repository)
        {
            XmlDocument doc = new XmlDocument();

            Assembly thisAssembly = this.GetType().Assembly;
            using (Stream xsd = thisAssembly.GetManifestResourceStream("TrueCraft.Core.Assets.TrueCraft.xsd")!)
                doc.Schemas.Add(XmlSchema.Read(xsd, null)!);

            using (Stream sz = thisAssembly.GetManifestResourceStream("TrueCraft.Core.Assets.TrueCraft.xml.gz")!)
            using (Stream s = new GZipStream(sz, CompressionMode.Decompress))
            using (XmlReader xmlr = XmlReader.Create(s))
            {
                doc.Load(xmlr);
                doc.Validate(null);
            }

            XmlNode truecraft = doc.ChildNodes.OfType<XmlNode>().Where<XmlNode>(n => n.LocalName == "truecraft").First<XmlNode>();
            XmlNode recipes = truecraft.ChildNodes.OfType<XmlNode>().Where<XmlNode>(n => n.LocalName == "recipes").First<XmlNode>();
            foreach (XmlNode recipe in recipes.ChildNodes)
                repository.RegisterRecipe(new CraftingRecipe(recipe));
        }


    }
}
