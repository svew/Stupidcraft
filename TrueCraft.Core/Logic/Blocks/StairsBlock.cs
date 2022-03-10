using System;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Blocks
{
    public abstract class StairsBlock : BlockProvider
    {
        public enum StairDirection
        {
            East = 0,
            West = 1,
            South = 2,
            North = 3
        }

        public override double Hardness { get { return 0; } }

        public override byte Luminance { get { return 0; } }

        public override bool Opaque { get { return false; } }

        public override byte LightOpacity { get { return 255; } }

        public override void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IDimension world, IRemoteClient user)
        {
            byte meta = 0;
            switch (MathHelper.DirectionByRotationFlat(user.Entity.Yaw))
            {
                case Direction.East:
                    meta = (byte)StairDirection.East;
                    break;
                case Direction.West:
                    meta = (byte)StairDirection.West;
                    break;
                case Direction.North:
                    meta = (byte)StairDirection.North;
                    break;
                case Direction.South:
                    meta = (byte)StairDirection.South;
                    break;
                default:
                    meta = 0; // Should never happen
                    break;
            }
            world.SetMetadata(descriptor.Coordinates, meta);
        }
    }

    public class WoodenStairsBlock : StairsBlock, IBurnableItem
    {
        public static readonly byte BlockID = 0x35;
        
        public override byte ID { get { return 0x35; } }
        
        public override double BlastResistance { get { return 15; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Wooden Stairs";
        }

        public override bool Flammable { get { return true; } }

        public TimeSpan BurnTime { get { return TimeSpan.FromSeconds(15); } }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Wood;
            }
        }
    }

    public class StoneStairsBlock : StairsBlock
    {
        public static readonly byte BlockID = 0x43;

        public override byte ID { get { return 0x43; } }

        public override double BlastResistance { get { return 30; } }

        public override string GetDisplayName(short metadata)
        {
            return "Stone Stairs";
        }
    }
}