using System;
using Moq;
using TrueCraft.Core.Logic;
using NUnit.Framework;
using System.Xml;
using System.Text;
using System.IO;

namespace TrueCraft.Core.Test.Logic
{
    [TestFixture]
    public class TestItemProvider
    {
        private static XmlNode GetTopNode(string xml)
        {
            XmlDocument doc = new XmlDocument();
            using (StringReader sr = new StringReader(xml))
            using (XmlReader xmlr = XmlReader.Create(sr))
                doc.Load(xmlr);

            return doc.FirstChild!;
        }

        [TestCase(332, 16, @"<item>
      <id>332</id>
      <maximumstack>16</maximumstack>
      <visiblemetadata>
        <metadata>
          <value>0</value>
        <displayname>Snowball</displayname>
        <icontexture>
          <x>14</x>
          <y>0</y>
        </icontexture>
        </metadata>
      </visiblemetadata>
    </item>
")]
        public void ctor(short expectedId, byte expectedMaxStack, string xml)
        {
            XmlNode itemNode = GetTopNode(xml);
            IItemProvider actual = new ItemProvider(itemNode);

            Assert.AreEqual(expectedId, actual.ID);
            Assert.AreEqual(expectedMaxStack, actual.MaximumStack);
        }
    }
}
