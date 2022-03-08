using System;

namespace TrueCraft.Core.Logic.Items
{
    public class MinecartItem : ItemProvider
    {
        public static readonly short ItemID = 0x148;

        public override short ID { get { return 0x148; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(7, 8);
        }

        public override sbyte MaximumStack { get { return 1; } }

        public override string GetDisplayName(short metadata)
        {
            return "Minecart";
        }
    }

    public class MinecartWithChestItem : MinecartItem
    {
        public static readonly new short ItemID = 0x156;

        public override short ID { get { return 0x156; } }

        public override string GetDisplayName(short metadata)
        {
            return "Minecart with Chest";
        }
    }

    public class MinecartWithFurnaceItem : MinecartItem
    {
        public static readonly new short ItemID = 0x157;

        public override short ID { get { return 0x157; } }

        public override string GetDisplayName(short metadata)
        {
            return "Minecart with Furnace";
        }
    }
}