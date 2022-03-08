using System;

namespace TrueCraft.Core.Logic.Blocks
{
    public class SpongeBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x13;
        
        public override byte ID { get { return 0x13; } }
        
        public override double BlastResistance { get { return 3; } }

        public override double Hardness { get { return 0.6; } }

        public override byte Luminance { get { return 0; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Sponge";
        }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Grass;
            }
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(0, 3);
        }
    }
}