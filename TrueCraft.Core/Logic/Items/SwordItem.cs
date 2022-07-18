using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class SwordItem : ToolItem
    {
        public SwordItem(XmlNode node) : base(node)
        {
        }

        public abstract float Damage { get; }

        public override ToolType ToolType
        {
            get
            {
                return ToolType.Sword;
            }
        }
    }

    public class WoodenSwordItem : SwordItem
    {
        public static readonly short ItemID = 0x10C;

        public WoodenSwordItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Wood; } }

        public override short BaseDurability { get { return 60; } }

        public override float Damage { get { return 2.5f; } }
    }

    public class StoneSwordItem : SwordItem
    {
        public static readonly short ItemID = 0x110;

        public StoneSwordItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Stone; } }

        public override short BaseDurability { get { return 132; } }

        public override float Damage { get { return 3.5f; } }
    }

    public class IronSwordItem : SwordItem
    {
        public static readonly short ItemID = 0x10B;

        public IronSwordItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Iron; } }

        public override short BaseDurability { get { return 251; } }

        public override float Damage { get { return 4.5f; } }
    }

    public class GoldenSwordItem : SwordItem
    {
        public static readonly short ItemID = 0x11B;

        public GoldenSwordItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Gold; } }

        public override short BaseDurability { get { return 33; } }

        public override float Damage { get { return 2.5f; } }
    }

    public class DiamondSwordItem : SwordItem
    {
        public static readonly short ItemID = 0x114;

        public DiamondSwordItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Diamond; } }

        public override short BaseDurability { get { return 1562; } }

        public override float Damage { get { return 5.5f; } }
    }
}