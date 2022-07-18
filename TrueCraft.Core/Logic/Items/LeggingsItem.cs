using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class LeggingsItem : ArmorItem
    {
        public LeggingsItem(XmlNode node) : base(node)
        {
        }
    }

    public class LeatherPantsItem : LeggingsItem
    {
        public static readonly short ItemID = 0x12C;

        public LeatherPantsItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Leather; } }

        public override short BaseDurability { get { return 46; } }

        public override float BaseArmor { get { return 3; } }
    }

    public class IronLeggingsItem : LeggingsItem
    {
        public static readonly short ItemID = 0x134;

        public IronLeggingsItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Iron; } }

        public override short BaseDurability { get { return 184; } }

        public override float BaseArmor { get { return 3; } }
    }

    public class GoldenLeggingsItem : LeggingsItem
    {
        public static readonly short ItemID = 0x13C;

        public GoldenLeggingsItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Gold; } }

        public override short BaseDurability { get { return 92; } }

        public override float BaseArmor { get { return 3; } }
    }

    public class DiamondLeggingsItem : LeggingsItem
    {
        public static readonly short ItemID = 0x138;

        public DiamondLeggingsItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Diamond; } }

        public override short BaseDurability { get { return 368; } }

        public override float BaseArmor { get { return 3; } }
    }

    public class ChainLeggingsItem : ArmorItem // Not HelmentItem because it can't inherit the recipe
    {
        public static readonly short ItemID = 0x130;

        public ChainLeggingsItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Chain; } }

        public override short BaseDurability { get { return 92; } }

        public override float BaseArmor { get { return 3; } }
    }
}