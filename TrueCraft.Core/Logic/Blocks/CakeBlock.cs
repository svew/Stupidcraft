using System;
using TrueCraft.Core.World;
using TrueCraft.Core.Networking;

namespace TrueCraft.Core.Logic.Blocks
{
    public class CakeBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x5C;
        
        public override byte ID { get { return 0x5C; } }
        
        public override double BlastResistance { get { return 2.5; } }

        public override double Hardness { get { return 0.5; } }

        public override byte Luminance { get { return 0; } }

        public override bool Opaque { get { return false; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Cake";
        }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Cloth;
            }
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(9, 7);
        }

        public override bool BlockRightClicked(BlockDescriptor descriptor, BlockFace face, IDimension world, IRemoteClient user)
        {
            if (descriptor.Metadata == 5)
                world.SetBlockID(descriptor.Coordinates, AirBlock.BlockID);
            else
                world.SetMetadata(descriptor.Coordinates, (byte)(descriptor.Metadata + 1));
            return false;
        }
    }
}