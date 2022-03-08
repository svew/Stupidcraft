using System;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class SwordItem : ToolItem
    {
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

        public override short ID { get { return 0x10C; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(0, 4);
        }

        public override ToolMaterial Material { get { return ToolMaterial.Wood; } }

        public override short BaseDurability { get { return 60; } }

        public override float Damage { get { return 2.5f; } }

        public override string GetDisplayName(short metadata)
        {
            return "Wooden Sword";
        }
    }

    public class StoneSwordItem : SwordItem
    {
        public static readonly short ItemID = 0x110;

        public override short ID { get { return 0x110; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(1, 4);
        }

        public override ToolMaterial Material { get { return ToolMaterial.Stone; } }

        public override short BaseDurability { get { return 132; } }

        public override float Damage { get { return 3.5f; } }

        public override string GetDisplayName(short metadata)
        {
            return "Stone Sword";
        }
    }

    public class IronSwordItem : SwordItem
    {
        public static readonly short ItemID = 0x10B;

        public override short ID { get { return 0x10B; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(2, 4);
        }

        public override ToolMaterial Material { get { return ToolMaterial.Iron; } }

        public override short BaseDurability { get { return 251; } }

        public override float Damage { get { return 4.5f; } }

        public override string GetDisplayName(short metadata)
        {
            return "Iron Sword";
        }
    }

    public class GoldenSwordItem : SwordItem
    {
        public static readonly short ItemID = 0x11B;

        public override short ID { get { return 0x11B; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(4, 4);
        }

        public override ToolMaterial Material { get { return ToolMaterial.Gold; } }

        public override short BaseDurability { get { return 33; } }

        public override float Damage { get { return 2.5f; } }

        public override string GetDisplayName(short metadata)
        {
            return "Golden Sword";
        }
    }

    public class DiamondSwordItem : SwordItem
    {
        public static readonly short ItemID = 0x114;

        public override short ID { get { return 0x114; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(3, 4);
        }

        public override ToolMaterial Material { get { return ToolMaterial.Diamond; } }

        public override short BaseDurability { get { return 1562; } }

        public override float Damage { get { return 5.5f; } }

        public override string GetDisplayName(short metadata)
        {
            return "Diamond Sword";
        }
    }
}