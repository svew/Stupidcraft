using System;
using TrueCraft.API.Logic;
using TrueCraft.API;
using TrueCraft.Core.Logic.Items;
using TrueCraft.API.World;
using TrueCraft.API.Networking;
using System.Linq;

namespace TrueCraft.Core.Logic.Blocks
{
    public class TorchBlock : BlockProvider
    {
        public enum TorchDirection
        {
            East = 0x01,
            West = 0x02,
            South = 0x03,
            North = 0x04,
            Ground = 0x05
        }

        public static readonly byte BlockID = 0x32;
        
        public override byte ID { get { return 0x32; } }
        
        public override double BlastResistance { get { return 0; } }

        public override double Hardness { get { return 0; } }

        public override byte Luminance { get { return 13; } }

        public override bool Opaque { get { return false; } }

        public override bool RenderOpaque { get { return true; } }
        
        public override string DisplayName { get { return "Torch"; } }

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
                return new BoundingBox(new Vector3(4 / 16.0, 0, 4 / 16.0), new Vector3(12 / 16.0, 7.0 / 16.0, 12 / 16.0));
            }
        }

        public override void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user)
        {
            TorchDirection[] preferredDirections =
            {
                TorchDirection.West, TorchDirection.East,
                TorchDirection.North, TorchDirection.South,
                TorchDirection.Ground
            };
            TorchDirection direction;
            switch (face)
            {
                case BlockFace.PositiveZ:
                    direction = TorchDirection.South;
                    break;
                case BlockFace.NegativeZ:
                    direction = TorchDirection.North;
                    break;
                case BlockFace.PositiveX:
                    direction = TorchDirection.East;
                    break;
                case BlockFace.NegativeX:
                    direction = TorchDirection.West;
                    break;
                default:
                    direction = TorchDirection.Ground;
                    break;
            }
            int i = 0;
            descriptor.Metadata = (byte)direction;
            while (!IsSupported(descriptor, user.Server, world) && i < preferredDirections.Length)
            {
                direction = preferredDirections[i++];
                descriptor.Metadata = (byte)direction;
            }
            world.SetBlockData(descriptor.Coordinates, descriptor);
        }

        public override void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IWorld world, IRemoteClient user)
        {
            coordinates += MathHelper.BlockFaceToCoordinates(face);
            var old = world.GetBlockData(coordinates);
            byte[] overwritable =
            {
                AirBlock.BlockID,
                WaterBlock.BlockID,
                StationaryWaterBlock.BlockID,
                LavaBlock.BlockID,
                StationaryLavaBlock.BlockID
            };
            if (overwritable.Any(b => b == old.ID))
            {
                var data = world.GetBlockData(coordinates);
                data.ID = ID;
                data.Metadata = (byte)item.Metadata;

                BlockPlaced(data, face, world, user);

                if (!IsSupported(world.GetBlockData(coordinates), user.Server, world))
                    world.SetBlockData(coordinates, old);
                else
                {
                    item.Count--;
                    user.Inventory[user.SelectedSlot] = item;
                }
            }
        }

        public override Vector3i GetSupportDirection(BlockDescriptor descriptor)
        {
            switch ((TorchDirection)descriptor.Metadata)
            {
                case TorchDirection.Ground:
                    return Vector3i.Down;
                case TorchDirection.East:
                    return Vector3i.West;
                case TorchDirection.West:
                    return Vector3i.East;
                case TorchDirection.North:
                    return Vector3i.South;
                case TorchDirection.South:
                    return Vector3i.North;
            }
            return Vector3i.Zero;
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(0, 5);
        }
    }
}
