using System;
using TrueCraft.Core.Logic.Items;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Blocks
{
    public class BedBlock : BlockProvider
    {
        [Flags]
        public enum BedDirection : byte
        {
            South =  0x0,
            West = 0x1,
            North =  0x2,
            East = 0x3,
        }

        [Flags]
        public enum BedType : byte
        {
            Foot = 0x0,
            Head = 0x8,
        }

        public static readonly byte BlockID = 0x1A;
        
        public override byte ID { get { return 0x1A; } }
        
        public override double BlastResistance { get { return 1; } }

        public override double Hardness { get { return 0.2; } }

        public override byte Luminance { get { return 0; } }

        public override bool Opaque { get { return false; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Bed";
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
            return new Tuple<int, int>(6, 8);
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new[] { new ItemStack(BedItem.ItemID) };
        }
            
        public bool ValidBedPosition(BlockDescriptor descriptor, IBlockRepository repository, IDimension dimension, bool checkNeighbor = true, bool checkSupport = false)
        {
            if (checkNeighbor)
            {
                var other = Vector3i.Zero;
                switch ((BedDirection)(descriptor.Metadata & 0x3))
                {
                    case BedDirection.East:
                        other = Vector3i.East;
                        break;
                    case BedDirection.West:
                        other = Vector3i.West;
                        break;
                    case BedDirection.North:
                        other = Vector3i.North;
                        break;
                    case BedDirection.South:
                        other = Vector3i.South;
                        break;
                }
                if ((descriptor.Metadata & (byte)BedType.Head) == (byte)BedType.Head)
                    other = -other;
                if (dimension.GetBlockID(descriptor.Coordinates + other) != BedBlock.BlockID)
                    return false;
            }
            if (checkSupport)
            {
                var supportingBlock = repository.GetBlockProvider(dimension.GetBlockID(descriptor.Coordinates + Vector3i.Down));
                if (!supportingBlock.Opaque)
                    return false;
            }
            return true;
        }

        public override void BlockUpdate(BlockDescriptor descriptor, BlockDescriptor source, IMultiplayerServer server, IDimension dimension)
        {
            if (!ValidBedPosition(descriptor, dimension.BlockRepository, dimension))
                dimension.SetBlockID(descriptor.Coordinates, 0);
            base.BlockUpdate(descriptor, source, server, dimension);
        }
    }
}