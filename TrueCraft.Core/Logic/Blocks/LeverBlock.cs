using System;

namespace TrueCraft.Core.Logic.Blocks
{
    public class LeverBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x45;
        
        public override byte ID { get { return 0x45; } }
        
        public override double BlastResistance { get { return 2.5; } }

        public override double Hardness { get { return 0.5; } }

        public override byte Luminance { get { return 0; } }

        public override bool Opaque { get { return false; } }
        
        public override string DisplayName { get { return "Lever"; } }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Wood;
            }
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(0, 6);
        }
    }
}