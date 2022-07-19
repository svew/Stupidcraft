using System;
using Moq;
using TrueCraft.Core.Logic;
using NUnit.Framework;
using System.Xml;
using System.Text;
using System.IO;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Test.Logic
{
    [TestFixture]
    public class TestArmorItem
    {
        private static XmlNode GetTopNode(string xml)
        {
            XmlDocument doc = new XmlDocument();
            using (StringReader sr = new StringReader(xml))
            using (XmlReader xmlr = XmlReader.Create(sr))
                doc.Load(xmlr);

            return doc.FirstChild!;
        }

        [TestCase(
            ArmorKind.Helmet, ArmorMaterial.Leather, 34, 1.5f,
            @"    <item>
      <id>298</id>
      <maximumstack>1</maximumstack>
      <visiblemetadata>
        <metadata>
          <value>0</value>
        <displayname>Leather Cap</displayname>
        <icontexture>
          <x>0</x>
          <y>0</y>
        </icontexture>
        </metadata>
      </visiblemetadata>
      <behavior>TrueCraft.Core.Logic.Items.ArmorItem</behavior>
      <armor>
        <kind>Helmet</kind>
        <material>Leather</material>
        <durability>34</durability>
        <defencepoints>1.5</defencepoints>
      </armor>
    </item>")]
        [TestCase(ArmorKind.Chestplate, ArmorMaterial.Chain, 96, 4f,
            @"    <item>
      <id>303</id>
      <maximumstack>1</maximumstack>
      <visiblemetadata>
        <metadata>
          <value>0</value>
        <displayname>Chain Chestplate</displayname>
        <icontexture>
          <x>1</x>
          <y>1</y>
        </icontexture>
        </metadata>
      </visiblemetadata>
      <behavior>TrueCraft.Core.Logic.Items.ArmorItem</behavior>
      <armor>
        <kind>Chestplate</kind>
        <material>Chain</material>
        <durability>96</durability>
        <defencepoints>4</defencepoints>
      </armor>
    </item>")]
        [TestCase(ArmorKind.Leggings, ArmorMaterial.Iron, 184, 3f, 
            @"    <item>
      <id>308</id>
      <maximumstack>1</maximumstack>
      <visiblemetadata>
        <metadata>
          <value>0</value>
        <displayname>Iron Leggings</displayname>
        <icontexture>
          <x>2</x>
          <y>2</y>
        </icontexture>
        </metadata>
      </visiblemetadata>
      <behavior>TrueCraft.Core.Logic.Items.ArmorItem</behavior>
      <armor>
        <kind>Leggings</kind>
        <material>Iron</material>
        <durability>184</durability>
        <defencepoints>3</defencepoints>
      </armor>
    </item>")]
        [TestCase(ArmorKind.Boots, ArmorMaterial.Gold, 80, 1.5f,
            @"    <item>
      <id>317</id>
      <maximumstack>1</maximumstack>
      <visiblemetadata>
        <metadata>
          <value>0</value>
        <displayname>Golden Boots</displayname>
        <icontexture>
          <x>4</x>
          <y>3</y>
        </icontexture>
        </metadata>
      </visiblemetadata>
      <behavior>TrueCraft.Core.Logic.Items.ArmorItem</behavior>
      <armor>
        <kind>Boots</kind>
        <material>Gold</material>
        <durability>80</durability>
        <defencepoints>1.5</defencepoints>
      </armor>
    </item>")]
        [TestCase(ArmorKind.Helmet, ArmorMaterial.Diamond, 272, 1.5f,
            @"    <item>
      <id>310</id>
      <maximumstack>1</maximumstack>
      <visiblemetadata>
        <metadata>
          <value>0</value>
        <displayname>Diamond Helmet</displayname>
        <icontexture>
          <x>3</x>
          <y>0</y>
        </icontexture>
        </metadata>
      </visiblemetadata>
      <behavior>TrueCraft.Core.Logic.Items.ArmorItem</behavior>
      <armor>
        <kind>Helmet</kind>
        <material>Diamond</material>
        <durability>272</durability>
        <defencepoints>1.5</defencepoints>
      </armor>
    </item>")]
        public void ctor(ArmorKind expectedKind, ArmorMaterial expectedMaterial,
            short expectedDurability, float expectedDefencePoints, string xml)
        {
            XmlNode itemNode = GetTopNode(xml);
            IArmorItem actual = new ArmorItem(itemNode);

            Assert.AreEqual(expectedKind, actual.Kind);
            Assert.AreEqual(expectedMaterial, actual.Material);
            Assert.AreEqual(expectedDurability, actual.Durability);
            Assert.AreEqual(expectedDefencePoints, actual.DefencePoints);
        }
    }
}
