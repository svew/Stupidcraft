using System;
using TrueCraft.Core.Logic.Items;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Blocks
{
    public class RedstoneDustBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x37;
        
        public override byte ID { get { return 0x37; } }
        
        public override double BlastResistance { get { return 0; } }

        public override double Hardness { get { return 0; } }

        public override byte Luminance { get { return 0; } }

        public override bool Opaque { get { return false; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Redstone Dust";
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(4, 10);
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new[] { new ItemStack(RedstoneItem.ItemID, 1, descriptor.Metadata) };
        }

        public override Vector3i GetSupportDirection(BlockDescriptor descriptor)
        {
            return Vector3i.Down;
        }
    }
}