using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic
{
    public class Discover : IDiscover
    {
        public Discover()
        {
        }

        public void DoDiscovery()
        {
            BlockRepository.Init(this);
            ItemRepository.Init(this);
            CraftingRepository.Init(this);
        }

        public void DiscoverBlockProviders(IRegisterBlockProvider repository)
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
                repository.RegisterBlockProvider(instance);
            });
        }

        public void DiscoverItemProviders(IRegisterItemProvider repository)
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
                repository.RegisterItemProvider(instance);
            });
        }


        public void DiscoverRecipes(IRegisterRecipe repository)
        {
            XmlDocument doc = new XmlDocument();

            Assembly api = AppDomain.CurrentDomain.GetAssemblies().Where<Assembly>(a => a.Location.EndsWith("TrueCraft.API.dll")).First<Assembly>();  // TODO do without Linq
            using (Stream xsd = api.GetManifestResourceStream("TrueCraft.API.Assets.TrueCraft.xsd"))
                doc.Schemas.Add(XmlSchema.Read(xsd, null));

            using (Stream sz = this.GetType().Assembly.GetManifestResourceStream("TrueCraft.Core.Assets.TrueCraft.xml.gz"))
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
