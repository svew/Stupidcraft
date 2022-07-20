using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class PickaxeItem : ToolItem
    {
        public PickaxeItem(XmlNode node) : base(node)
        {
        }

        public override ToolType ToolType
        {
            get
            {
                return ToolType.Pickaxe;
            }
        }
    }

    public class WoodenPickaxeItem : PickaxeItem
    {
        public static readonly short ItemID = 0x10E;

        public WoodenPickaxeItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Wood; } }

        public override short Durability { get { return 60; } }
    }

    public class StonePickaxeItem : PickaxeItem
    {
        public static readonly short ItemID = 0x112;

        public StonePickaxeItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Stone; } }

        public override short Durability { get { return 132; } }
    }

    public class IronPickaxeItem : PickaxeItem
    {
        public static readonly short ItemID = 0x101;

        public IronPickaxeItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Iron; } }

        public override short Durability { get { return 251; } }
    }

    public class GoldenPickaxeItem : PickaxeItem
    {
        public static readonly short ItemID = 0x11D;

        public GoldenPickaxeItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Gold; } }

        public override short Durability { get { return 33; } }
    }

    public class DiamondPickaxeItem : PickaxeItem
    {
        public static readonly short ItemID = 0x116;

        public DiamondPickaxeItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Diamond; } }

        public override short Durability { get { return 1562; } }
    }
}