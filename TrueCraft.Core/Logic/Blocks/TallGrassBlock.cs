using System;
using TrueCraft.Core.Logic.Items;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Blocks
{
    public class TallGrassBlock : BlockProvider
    {
        public enum TallGrassType
        {
            DeadBush = 0,
            TallGrass = 1,
            Fern = 2
        }

        public static readonly byte BlockID = 0x1F;
        
        public override byte ID { get { return 0x1F; } }
        
        public override double BlastResistance { get { return 0; } }

        public override double Hardness { get { return 0; } }

        public override byte Luminance { get { return 0; } }

        public override bool Opaque { get { return false; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Tall Grass";
        }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Grass;
            }
        }

        public override bool Flammable { get { return true; } }

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
            return new Tuple<int, int>(7, 2);
        }
        
        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            if (MathHelper.Random.Next (1, 24) == 1)
                return new[] { new ItemStack (SeedsItem.ItemID, 1) };
            else
                return new[] { ItemStack.EmptyStack };
        }
    }
}
