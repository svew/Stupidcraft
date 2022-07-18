using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class BootsItem : ArmorItem
    {
        public BootsItem(XmlNode node) : base(node)
        {
        }
    }

    public class LeatherBootsItem : BootsItem
    {
        public static readonly short ItemID = 0x12D;

        public LeatherBootsItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Leather; } }

        public override short BaseDurability { get { return 40; } }

        public override float BaseArmor { get { return 1.5f; } }
    }

    public class IronBootsItem : BootsItem
    {
        public static readonly short ItemID = 0x135;

        public IronBootsItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Iron; } }

        public override short BaseDurability { get { return 160; } }

        public override float BaseArmor { get { return 1.5f; } }
    }

    public class GoldenBootsItem : BootsItem
    {
        public static readonly short ItemID = 0x13D;

        public GoldenBootsItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Gold; } }

        public override short BaseDurability { get { return 80; } }

        public override float BaseArmor { get { return 1.5f; } }
    }

    public class DiamondBootsItem : BootsItem
    {
        public static readonly short ItemID = 0x139;

        public DiamondBootsItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Diamond; } }

        public override short BaseDurability { get { return 320; } }

        public override float BaseArmor { get { return 1.5f; } }
    }

    public class ChainBootsItem : BootsItem
    {
        public static readonly short ItemID = 0x131;

        public ChainBootsItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Chain; } }

        public override short BaseDurability { get { return 79; } }

        public override float BaseArmor { get { return 1.5f; } }
    }
}