using System;

namespace TrueCraft.Core.Logic.Blocks
{
    public class SlabBlock : BlockProvider
    {
        public enum SlabMaterial
        {
            Stone       = 0x0,
            Standstone  = 0x1,
            Wooden      = 0x2,
            Cobblestone = 0x3
        }

        public static readonly byte BlockID = 0x2C;
        
        public override byte ID { get { return 0x2C; } }
        
        public override double BlastResistance { get { return 30; } }

        public override double Hardness { get { return 2; } }

        public override byte Luminance { get { return 0; } }

        public override bool Opaque { get { return false; } }

        public override byte LightOpacity { get { return 255; } }

        public override string GetDisplayName(short metadata)
        {
            return "Stone Slab";
        }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Wood; // TODO: Deal with metadata god dammit
            }
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(6, 0);
        }
    }

    public class DoubleSlabBlock : SlabBlock
    {
        public static readonly new byte BlockID = 0x2B;

        public override byte ID { get { return 0x2B; } }

        public override double BlastResistance { get { return 30; } }

        public override double Hardness { get { return 2; } }

        public override byte Luminance { get { return 0; } }

        public override string GetDisplayName(short metadata)
        {
            return "Double Stone Slab";
        }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Wood;
            }
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(6, 0);
        }
    }
}