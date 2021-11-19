using System;

namespace TrueCraft.Core.Logic.Items
{
    public class GoldenAppleItem : FoodItem
    {
        public static readonly short ItemID = 0x142;

        public override short ID { get { return 0x142; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(11, 0);
        }

        public override float Restores { get { return 10; } }

        public override string DisplayName { get { return "Golden Apple"; } }
    }
}