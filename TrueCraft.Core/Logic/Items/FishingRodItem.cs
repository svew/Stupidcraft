using System;

namespace TrueCraft.Core.Logic.Items
{
    public class FishingRodItem : ToolItem
    {
        public static readonly short ItemID = 0x15A;

        public override short ID { get { return 0x15A; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(5, 4);
        }

        public override sbyte MaximumStack { get { return 1; } }

        public override short BaseDurability { get { return 65; } }

        public override string DisplayName { get { return "Fishing Rod"; } }
    }
}