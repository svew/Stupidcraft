using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class ShovelItem : ToolItem
    {
        public ShovelItem(XmlNode node) : base(node)
        {
        }

        public override ToolType ToolType
        {
            get
            {
                return ToolType.Shovel;
            }
        }
    }

    public class WoodenShovelItem : ShovelItem
    {
        public static readonly short ItemID = 0x10D;

        public WoodenShovelItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Wood; } }

        public override short Durability { get { return 60; } }
    }

    public class StoneShovelItem : ShovelItem
    {
        public static readonly short ItemID = 0x111;

        public StoneShovelItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Stone; } }

        public override short Durability { get { return 132; } }
    }

    public class IronShovelItem : ShovelItem
    {
        public static readonly short ItemID = 0x100;

        public IronShovelItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Iron; } }

        public override short Durability { get { return 251; } }
    }

    public class GoldenShovelItem : ShovelItem
    {
        public static readonly short ItemID = 0x11C;

        public GoldenShovelItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Gold; } }

        public override short Durability { get { return 33; } }
    }

    public class DiamondShovelItem : ShovelItem
    {
        public static readonly short ItemID = 0x115;

        public DiamondShovelItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Diamond; } }

        public override short Durability { get { return 1562; } }
    }
}