using System;
using NUnit.Framework;
using System.Xml;
using System.IO;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Test.Logic.Items
{
    [TestFixture]
    public class TestFoodItem
    {
        private static XmlNode GetTopNode(string xml)
        {
            XmlDocument doc = new XmlDocument();
            using (StringReader sr = new StringReader(xml))
            using (XmlReader xmlr = XmlReader.Create(sr))
                doc.Load(xmlr);

            return doc.FirstChild!;
        }

        [TestCase(2, @"<item>
      <id>260</id>
      <maximumstack>1</maximumstack>
      <visiblemetadata>
        <metadata>
          <value>0</value>
        <displayname>Apple</displayname>
        <icontexture>
          <x>10</x>
          <y>0</y>
        </icontexture>
        </metadata>
      </visiblemetadata>
      <behavior>TrueCraft.Core.Logic.Items.FoodItem</behavior>
      <food>
        <restores>2</restores>
      </food>
    </item>
")]
        public void ctor(float expectedRestores, string xml)
        {
            XmlNode itemNode = GetTopNode(xml);
            IFoodItem actual = new FoodItem(itemNode);

            Assert.AreEqual(expectedRestores, actual.Restores);
        }
    }
}
