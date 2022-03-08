using System;

namespace TrueCraft.Core.Logic.Items
{
    public class BoatItem : ItemProvider
    {
        public static readonly short ItemID = 0x14D;

        public override short ID { get { return 0x14D; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(8, 8);
        }

        public override sbyte MaximumStack { get { return 1; } }

        public override string GetDisplayName(short metadata)
        {
            return "Boat";
        }
    }
}