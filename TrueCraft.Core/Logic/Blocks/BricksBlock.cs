using System;

namespace TrueCraft.Core.Logic.Blocks
{
    public class BricksBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x2D;
        
        public override byte ID { get { return 0x2D; } }
        
        public override double BlastResistance { get { return 30; } }

        public override double Hardness { get { return 2; } }

        public override byte Luminance { get { return 0; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Bricks";
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(7, 0);
        }
    }
}