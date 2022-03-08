using System;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Items
{
    public class BedItem : ItemProvider
    {
        public static readonly short ItemID = 0x163;

        public override short ID { get { return 0x163; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(13, 2);
        }

        public override sbyte MaximumStack { get { return 1; } }

        public override string GetDisplayName(short metadata)
        {
            return "Bed";
        }

        public override void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IWorld world, IRemoteClient user)
        {
            ServerOnly.Assert();

            coordinates += MathHelper.BlockFaceToCoordinates(face);
            var head = coordinates;
            var foot = coordinates;
            BedBlock.BedDirection direction = BedBlock.BedDirection.North;
            switch (MathHelper.DirectionByRotationFlat(user.Entity.Yaw))
            {
                case Direction.North:
                    head += Vector3i.North;
                    direction = BedBlock.BedDirection.North;
                    break;
                case Direction.South:
                    head += Vector3i.South;
                    direction = BedBlock.BedDirection.South;
                    break;
                case Direction.East:
                    head += Vector3i.East;
                    direction = BedBlock.BedDirection.East;
                    break;
                case Direction.West:
                    head += Vector3i.West;
                    direction = BedBlock.BedDirection.West;
                    break;
            }
            var bedProvider = (BedBlock)user.Server.BlockRepository.GetBlockProvider(BedBlock.BlockID);
            if (!bedProvider.ValidBedPosition(new BlockDescriptor { Coordinates = head },
                user.Server.BlockRepository, user.World, false, true) ||
                !bedProvider.ValidBedPosition(new BlockDescriptor { Coordinates = foot },
                user.Server.BlockRepository, user.World, false, true))
            {
                return;
            }
            user.Server.BlockUpdatesEnabled = false;
            world.SetBlockData(head, new BlockDescriptor
                { ID = BedBlock.BlockID, Metadata = (byte)((byte)direction | (byte)BedBlock.BedType.Head) });
            world.SetBlockData(foot, new BlockDescriptor
                { ID = BedBlock.BlockID, Metadata = (byte)((byte)direction | (byte)BedBlock.BedType.Foot) });
            user.Server.BlockUpdatesEnabled = true;
            item.Count--;
            user.Inventory[user.SelectedSlot].Item = item;
        }
    }
}