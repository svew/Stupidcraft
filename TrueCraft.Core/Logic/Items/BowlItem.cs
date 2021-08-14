using System;
using TrueCraft.API.Logic;
using TrueCraft.API;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.Logic.Items
{
    public class BowlItem : ItemProvider
    {
        public static readonly short ItemID = 0x119;

        public override short ID { get { return 0x119; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(7, 4);
        }

        public override string DisplayName { get { return "Bowl"; } }
    }
}