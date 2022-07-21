using System;
using System.IO;
using System.Xml;
using NUnit.Framework;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Core.Test.Logic.Items
{
    public class TestToolItem
    {
        [TestCase(ToolType.Pickaxe, ToolMaterial.Wood, 60, 0,
            @"    <item>
      <id>270</id>
      <maximumstack>1</maximumstack>
      <visiblemetadata>
        <metadata>
          <value>0</value>
        <displayname>Wooden Pickaxe</displayname>
        <icontexture>
          <x>0</x>
          <y>6</y>
        </icontexture>
        </metadata>
      </visiblemetadata>
      <behavior>TrueCraft.Core.Logic.Items.PickaxeItem</behavior>
      <tool>
        <kind>Pickaxe</kind>
        <material>Wood</material>
        <durability>60</durability>
        <damage>0</damage>
      </tool>
    </item>
")]
        [TestCase(ToolType.Axe, ToolMaterial.Stone, 132, 0,
            @"    <item>
      <id>275</id>
      <maximumstack>1</maximumstack>
      <visiblemetadata>
        <metadata>
          <value>0</value>
        <displayname>Stone Axe</displayname>
        <icontexture>
          <x>1</x>
          <y>7</y>
        </icontexture>
        </metadata>
      </visiblemetadata>
      <behavior>TrueCraft.Core.Logic.Items.AxeItem</behavior>
      <tool>
        <kind>Axe</kind>
        <material>Stone</material>
        <durability>132</durability>
        <damage>0</damage>
      </tool>
    </item>
")]
        [TestCase(ToolType.Shovel, ToolMaterial.Iron, 251, 0,
            @"    <item>
      <id>256</id>
      <maximumstack>1</maximumstack>
      <visiblemetadata>
        <metadata>
          <value>0</value>
        <displayname>Iron Shovel</displayname>
        <icontexture>
          <x>2</x>
          <y>5</y>
        </icontexture>
        </metadata>
      </visiblemetadata>
      <behavior>TrueCraft.Core.Logic.Items.ShovelItem</behavior>
      <tool>
        <kind>Shovel</kind>
        <material>Iron</material>
        <durability>251</durability>
        <damage>0</damage>
      </tool>
    </item>
")]
        [TestCase(ToolType.Hoe, ToolMaterial.Gold, 33, 0,
            @"    <item>
      <id>294</id>
      <maximumstack>1</maximumstack>
      <visiblemetadata>
        <metadata>
          <value>0</value>
        <displayname>Golden Hoe</displayname>
        <icontexture>
          <x>4</x>
          <y>8</y>
        </icontexture>
        </metadata>
      </visiblemetadata>
      <behavior>TrueCraft.Core.Logic.Items.HoeItem</behavior>
      <tool>
        <kind>Hoe</kind>
        <material>Gold</material>
        <durability>33</durability>
        <damage>0</damage>
      </tool>
    </item>
")]
        [TestCase(ToolType.Sword, ToolMaterial.Diamond, 1562, 5.5,
            @"    <item>
      <id>276</id>
      <maximumstack>1</maximumstack>
      <visiblemetadata>
        <metadata>
          <value>0</value>
        <displayname>Diamond Sword</displayname>
        <icontexture>
          <x>3</x>
          <y>4</y>
        </icontexture>
        </metadata>
      </visiblemetadata>
      <behavior>TrueCraft.Core.Logic.Items.SwordItem</behavior>
      <tool>
        <kind>Sword</kind>
        <material>Diamond</material>
        <durability>1562</durability>
        <damage>5.5</damage>
      </tool>
    </item>
")]
        public void ctor(ToolType expectedKind, ToolMaterial expectedMaterial,
            short expectedDurability, float expectedDamage, string xml)
        {
            XmlNode itemNode = Utility.GetTopNode(xml);
            IToolItem actual = new AToolItem(itemNode);

            Assert.AreEqual(expectedKind, actual.ToolType);
            Assert.AreEqual(expectedMaterial, actual.Material);
            Assert.AreEqual(expectedDurability, actual.Durability);
            Assert.AreEqual(expectedDamage, actual.Damage);
        }

        // The ToolItem constructor is protected, so this sub-class
        // allows us to test the protected constructor.
        private class AToolItem : ToolItem
        {
            public AToolItem(XmlNode node) : base(node)
            {
            }
        }
    }
}

