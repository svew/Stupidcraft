using System;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class AxeItem : ToolItem
    {
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

        public override short ID { get { return 0x10F; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(0, 7);
        }

        public override ToolMaterial Material { get { return ToolMaterial.Wood; } }

        public override short BaseDurability { get { return 60; } }

        public override string DisplayName { get { return "Wooden Axe"; } }
    }

    public class StoneAxeItem : AxeItem
    {
        public static readonly short ItemID = 0x113;

        public override short ID { get { return 0x113; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(1, 7);
        }

        public override ToolMaterial Material { get { return ToolMaterial.Stone; } }

        public override short BaseDurability { get { return 132; } }

        public override string DisplayName { get { return "Stone Axe"; } }
    }

    public class IronAxeItem : AxeItem
    {
        public static readonly short ItemID = 0x102;

        public override short ID { get { return 0x102; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(2, 7);
        }

        public override ToolMaterial Material { get { return ToolMaterial.Iron; } }

        public override short BaseDurability { get { return 251; } }

        public override string DisplayName { get { return "Iron Axe"; } }
    }

    public class GoldenAxeItem : AxeItem
    {
        public static readonly short ItemID = 0x11E;

        public override short ID { get { return 0x11E; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(4, 7);
        }

        public override ToolMaterial Material { get { return ToolMaterial.Gold; } }

        public override short BaseDurability { get { return 33; } }

        public override string DisplayName { get { return "Golden Axe"; } }
    }

    public class DiamondAxeItem : AxeItem
    {
        public static readonly short ItemID = 0x117;

        public override short ID { get { return 0x117; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(3, 7);
        }

        public override ToolMaterial Material { get { return ToolMaterial.Diamond; } }

        public override short BaseDurability { get { return 1562; } }

        public override string DisplayName { get { return "Diamond Axe"; } }
    }
}