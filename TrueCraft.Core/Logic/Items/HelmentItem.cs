using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class HelmetItem : ArmorItem
    {
        public HelmetItem(XmlNode node) : base(node)
        {
        }
    }

    public class LeatherCapItem : HelmetItem
    {
        public static readonly short ItemID = 0x12A;

        public LeatherCapItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Leather; } }

        public override short BaseDurability { get { return 34; } }

        public override float BaseArmor { get { return 1.5f; } }
    }

    public class IronHelmetItem : HelmetItem
    {
        public static readonly short ItemID = 0x132;

        public IronHelmetItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Iron; } }

        public override short BaseDurability { get { return 136; } }

        public override float BaseArmor { get { return 1.5f; } }
    }

    public class GoldenHelmetItem : HelmetItem
    {
        public static readonly short ItemID = 0x13A;

        public GoldenHelmetItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Gold; } }

        public override short BaseDurability { get { return 68; } }

        public override float BaseArmor { get { return 1.5f; } }
    }

    public class DiamondHelmetItem : HelmetItem
    {
        public static readonly short ItemID = 0x136;

        public DiamondHelmetItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Diamond; } }

        public override short BaseDurability { get { return 272; } }

        public override float BaseArmor { get { return 1.5f; } }
    }

    public class ChainHelmetItem : ArmorItem // Not HelmentItem because it can't inherit the recipe
    {
        public static readonly short ItemID = 0x12E;

        public ChainHelmetItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Chain; } }

        public override short BaseDurability { get { return 67; } }

        public override float BaseArmor { get { return 1.5f; } }
    }
}