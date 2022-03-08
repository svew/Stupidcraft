using System;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Blocks
{
    public class DeadBushBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x20;
        
        public override byte ID { get { return 0x20; } }
        
        public override double BlastResistance { get { return 0; } }

        public override double Hardness { get { return 0; } }

        public override byte Luminance { get { return 0; } }

        public override bool Opaque { get { return false; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Dead Bush";
        }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Grass;
            }
        }

        public override BoundingBox? BoundingBox { get { return null; } }

        public override BoundingBox? InteractiveBoundingBox
        {
            get
            {
                return new BoundingBox(new Vector3(4 / 16.0), Vector3.One);
            }
        }

        public override Vector3i GetSupportDirection(BlockDescriptor descriptor)
        {
            return Vector3i.Down;
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(7, 3);
        }
    }
}