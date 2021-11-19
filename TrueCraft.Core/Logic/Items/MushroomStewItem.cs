using System;

namespace TrueCraft.Core.Logic.Items
{
    public class MushroomStewItem : FoodItem
    {
        public static readonly short ItemID = 0x11A;

        public override short ID { get { return 0x11A; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(8, 4);
        }

        public override sbyte MaximumStack { get { return 1; } }

        public override float Restores { get { return 5; } }

        public override string DisplayName { get { return "Mushroom Stew"; } }
    }
}