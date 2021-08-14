using System;
using TrueCraft.API.Logic;
using TrueCraft.API;
using TrueCraft.Core.Logic.Blocks;

namespace TrueCraft.Core.Logic.Items
{
    public class IronIngotItem : ItemProvider
    {
        public static readonly short ItemID = 0x109;

        public override short ID { get { return 0x109; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(7, 1);
        }

        public override string DisplayName { get { return "Iron Ingot"; } }
    }
}