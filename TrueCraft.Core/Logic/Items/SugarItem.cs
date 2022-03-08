using System;

namespace TrueCraft.Core.Logic.Items
{
    public class SugarItem : ItemProvider
    {
        public static readonly short ItemID = 0x161;

        public override short ID { get { return 0x161; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(13, 0);
        }

        public override string GetDisplayName(short metadata)
        {
            return "Sugar";
        }
    }
}