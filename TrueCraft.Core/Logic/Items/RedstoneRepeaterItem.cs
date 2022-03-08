using System;

namespace TrueCraft.Core.Logic.Items
{
    public class RedstoneRepeaterItem : ItemProvider
    {
        public static readonly short ItemID = 0x164;

        public override short ID { get { return 0x164; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(6, 5);
        }

        public override string GetDisplayName(short metadata)
        {
            return "Redstone Repeater";
        }
    }
}