using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class ChestplateItem : ArmorItem
    {
        public ChestplateItem(XmlNode node) : base(node)
        {
        }
    }

    public class LeatherTunicItem : ChestplateItem
    {
        public static readonly short ItemID = 0x12B;

        public LeatherTunicItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Leather; } }

        public override short BaseDurability { get { return 49; } }

        public override float BaseArmor { get { return 4; } }
    }

    public class IronChestplateItem : ChestplateItem
    {
        public static readonly short ItemID = 0x133;

        public IronChestplateItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Iron; } }

        public override short BaseDurability { get { return 192; } }

        public override float BaseArmor { get { return 4; } }
    }

    public class GoldenChestplateItem : ChestplateItem
    {
        public static readonly short ItemID = 0x13B;

        public GoldenChestplateItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Gold; } }

        public override short BaseDurability { get { return 96; } }

        public override float BaseArmor { get { return 4; } }
    }

    public class DiamondChestplateItem : ChestplateItem
    {
        public static readonly short ItemID = 0x137;

        public DiamondChestplateItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Diamond; } }

        public override short BaseDurability { get { return 384; } }

        public override float BaseArmor { get { return 4; } }
    }

    public class ChainChestplateItem : ArmorItem // Not HelmentItem because it can't inherit the recipe
    {
        public static readonly short ItemID = 0x12F;

        public ChainChestplateItem(XmlNode node) : base(node)
        {
        }

        public override ArmorMaterial Material { get { return ArmorMaterial.Chain; } }

        public override short BaseDurability { get { return 96; } }

        public override float BaseArmor { get { return 4; } }
    }
}