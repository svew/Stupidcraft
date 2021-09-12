using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using TrueCraft.API;
using TrueCraft.API.Windows;
using TrueCraft.API.Logic;
using System.Reflection;

namespace TrueCraft.Core.Logic
{
    public class CraftingRepository : ICraftingRepository
    {
        private readonly List<ICraftingRecipe> _recipes;

        private static CraftingRepository _singleton;

        private CraftingRepository()
        {
            _recipes = new List<ICraftingRecipe>();
        }

        public static ICraftingRepository Get()
        {
            if (object.ReferenceEquals(_singleton, null))
            {
                _singleton = new CraftingRepository();
                _singleton.DiscoverRecipes();
            }

            return _singleton;
        }

        private void DiscoverRecipes()
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
                _recipes.Add(new CraftingRecipe(recipe));
        }

        public ICraftingRecipe GetRecipe(CraftingPattern pattern)
        {
            foreach (ICraftingRecipe r in _recipes)
                if (r.Pattern == pattern)
                    return r;

            return null;
        }
    }
}