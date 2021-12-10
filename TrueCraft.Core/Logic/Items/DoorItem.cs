using System;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Items
{
    public abstract class DoorItem : ItemProvider
    {
        [Flags]
        public enum DoorFlags
        {
            Northeast = 0x0,
            Southeast = 0x1,
            Southwest = 0x2,
            Northwest = 0x3,
            Lower = 0x0,
            Upper = 0x8,
            Closed = 0x0,
            Open = 0x4
        }

        protected abstract byte BlockID { get; }

        public override sbyte MaximumStack { get { return 1; } }

        public override void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IWorld world, IRemoteClient user)
        {
            ServerOnly.Assert();

            var bottom = coordinates + MathHelper.BlockFaceToCoordinates(face);
            var top = bottom + Vector3i.Up;
            if (world.GetBlockID(top) != 0 || world.GetBlockID(bottom) != 0)
                return;
            DoorFlags direction;
            switch (MathHelper.DirectionByRotationFlat(user.Entity.Yaw))
            {
                case Direction.North:
                    direction = DoorFlags.Northwest;
                    break;
                case Direction.South:
                    direction = DoorFlags.Southeast;
                    break;
                case Direction.East:
                    direction = DoorFlags.Northeast;
                    break;
                default: // Direction.West:
                    direction = DoorFlags.Southwest;
                    break;
            }
            user.Server.BlockUpdatesEnabled = false;
            world.SetBlockID(bottom, BlockID);
            world.SetMetadata(bottom, (byte)direction);
            world.SetBlockID(top, BlockID);
            world.SetMetadata(top, (byte)(direction | DoorFlags.Upper));
            user.Server.BlockUpdatesEnabled = true;
            item.Count--;
            user.Hotbar[user.SelectedSlot].Item = item;
        }
    }

    public class IronDoorItem : DoorItem
    {
        public static readonly short ItemID = 0x14A;

        public override short ID { get { return 0x14A; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(12, 2);
        }

        public override string DisplayName { get { return "Iron Door"; } }

        protected override byte BlockID { get { return IronDoorBlock.BlockID; } }
    }

    public class WoodenDoorItem : DoorItem
    {
        public static readonly short ItemID = 0x144;

        public override short ID { get { return 0x144; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(11, 2);
        }

        public override string DisplayName { get { return "Wooden Door"; } }

        protected override byte BlockID { get { return WoodenDoorBlock.BlockID; } }
    }
}