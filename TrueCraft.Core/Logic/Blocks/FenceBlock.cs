using System;

namespace TrueCraft.Core.Logic.Blocks
{
    public class FenceBlock : BlockProvider, IBurnableItem
    {
        public static readonly byte BlockID = 0x55;
        
        public override byte ID { get { return 0x55; } }
        
        public override double BlastResistance { get { return 15; } }

        public override double Hardness { get { return 2; } }

        public override byte Luminance { get { return 0; } }

        public override bool Opaque { get { return false; } }

        public override bool Flammable { get { return true; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Fence";
        }

        public TimeSpan BurnTime { get { return TimeSpan.FromSeconds(15); } }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Wood;
            }
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(4, 0);
        }
    }
}