using System;
using TrueCraft.API.Logic;
using TrueCraft.API;

namespace TrueCraft.Core.Logic.Items
{
    public class CookieItem : FoodItem
    {
        public static readonly short ItemID = 0x165;

        public override short ID { get { return 0x165; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(12, 5);
        }

        public override sbyte MaximumStack { get { return 8; } }

        public override float Restores { get { return 0.5f; } }

        public override string DisplayName { get { return "Cookie"; } }
    }
}