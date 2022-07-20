using System;
using System.Xml;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class AxeItem : ToolItem
    {
        protected AxeItem(XmlNode node) : base(node)
        {
        }

        public override ToolType ToolType
        {
            get
            {
                return ToolType.Axe;
            }
        }
    }

    public class WoodenAxeItem : AxeItem
    {
        public static readonly short ItemID = 0x10F;

        public WoodenAxeItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Wood; } }

        public override short Durability { get { return 60; } }
    }

    public class StoneAxeItem : AxeItem
    {
        public static readonly short ItemID = 0x113;

        public StoneAxeItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Stone; } }

        public override short Durability { get { return 132; } }
    }

    public class IronAxeItem : AxeItem
    {
        public static readonly short ItemID = 0x102;

        public IronAxeItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Iron; } }

        public override short Durability { get { return 251; } }
    }

    public class GoldenAxeItem : AxeItem
    {
        public static readonly short ItemID = 0x11E;

        public GoldenAxeItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Gold; } }

        public override short Durability { get { return 33; } }
    }

    public class DiamondAxeItem : AxeItem
    {
        public static readonly short ItemID = 0x117;

        public DiamondAxeItem(XmlNode node) : base(node)
        {
        }

        public override ToolMaterial Material { get { return ToolMaterial.Diamond; } }

        public override short Durability { get { return 1562; } }
    }
}