using System;
using TrueCraft.API.Logic;
using TrueCraft.API;

namespace TrueCraft.Core.Logic.Items
{
    public class PaperItem : ItemProvider
    {
        public static readonly short ItemID = 0x153;

        public override short ID { get { return 0x153; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(10, 3);
        }

        public override string DisplayName { get { return "Paper"; } }
    }
}