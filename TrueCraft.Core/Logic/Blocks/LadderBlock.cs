using System;
using TrueCraft.Core.World;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Server;

namespace TrueCraft.Core.Logic.Blocks
{
    public class LadderBlock : BlockProvider
    {
        /// <summary>
        /// The side of the block that this ladder is attached to (i.e. "the north side")
        /// </summary>
        public enum LadderDirection
        {
            East = 0x04,
            West = 0x05,
            North = 0x03,
            South = 0x02
        }

        public static readonly byte BlockID = 0x41;
        
        public override byte ID { get { return 0x41; } }
        
        public override double BlastResistance { get { return 2; } }

        public override double Hardness { get { return 0.4; } }

        public override byte Luminance { get { return 0; } }

        public override bool Opaque { get { return false; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Ladder";
        }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Wood;
            }
        }

        public override BoundingBox? BoundingBox { get { return null; } }

        public override BoundingBox? InteractiveBoundingBox
        {
            get
            {
                return new BoundingBox(new Vector3(0.25, 0, 0.25), new Vector3(0.75, 0.5, 0.75));
            }
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(3, 5);
        }

        public override Vector3i GetSupportDirection(BlockDescriptor descriptor)
        {
            switch ((LadderDirection)descriptor.Metadata)
            {
                case LadderDirection.East:
                    return Vector3i.East;
                case LadderDirection.West:
                    return Vector3i.West;
                case LadderDirection.North:
                    return Vector3i.North;
                case LadderDirection.South:
                    return Vector3i.South;
                default:
                    return Vector3i.Zero;
            }
        }

        public override void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IDimension dimension, IRemoteClient user)
        {
            ServerOnly.Assert();

            coordinates += MathHelper.BlockFaceToCoordinates(face);
            var descriptor = dimension.GetBlockData(coordinates);
            LadderDirection direction;
            switch (MathHelper.DirectionByRotationFlat(user.Entity!.Yaw))
            {
                case Direction.North:
                    direction = LadderDirection.North;
                    break;
                case Direction.South:
                    direction = LadderDirection.South;
                    break;
                case Direction.East:
                    direction = LadderDirection.East;
                    break;
                default:
                    direction = LadderDirection.West;
                    break;
            }
            descriptor.Metadata = (byte)direction;
            if (IsSupported(dimension, descriptor))
            {
                dimension.SetBlockID(descriptor.Coordinates, BlockID);
                dimension.SetMetadata(descriptor.Coordinates, (byte)direction);
                item.Count--;
                user.Hotbar[user.SelectedSlot].Item = item;
            }
        }
    }
}