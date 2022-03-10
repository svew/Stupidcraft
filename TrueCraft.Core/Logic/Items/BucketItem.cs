using System;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Items
{
    public class BucketItem : ToolItem
    {
        public static readonly short ItemID = 0x145;

        public override short ID { get { return 0x145; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(10, 4);
        }

        public override string GetDisplayName(short metadata)
        {
            return "Bucket";
        }

        protected virtual byte? RelevantBlockType { get { return null; } }

        public override void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IDimension world, IRemoteClient user)
        {
            ServerOnly.Assert();

            coordinates += MathHelper.BlockFaceToCoordinates(face);
            if (item.ID == ItemID) // Empty bucket
            {
                var block = world.GetBlockID(coordinates);
                if (block == WaterBlock.BlockID || block == StationaryWaterBlock.BlockID)
                {
                    var meta = world.GetMetadata(coordinates);
                    if (meta == 0) // Is source block?
                    {
                        user.Hotbar[user.SelectedSlot].Item = new ItemStack(WaterBucketItem.ItemID);
                        world.SetBlockID(coordinates, 0);
                    }
                }
                else if (block == LavaBlock.BlockID || block == StationaryLavaBlock.BlockID)
                {
                    var meta = world.GetMetadata(coordinates);
                    if (meta == 0) // Is source block?
                    {
                        user.Hotbar[user.SelectedSlot].Item = new ItemStack(LavaBucketItem.ItemID);
                        world.SetBlockID(coordinates, 0);
                    }
                }
            }
            else
            {
                var provider = user.Server.BlockRepository.GetBlockProvider(world.GetBlockID(coordinates));
                if (!provider.Opaque)
                {
                    if (RelevantBlockType != null)
                    {
                        var blockType = RelevantBlockType.Value;
                        user.Server.BlockUpdatesEnabled = false;
                        world.SetBlockID(coordinates, blockType);
                        world.SetMetadata(coordinates, 0); // Source block
                        user.Server.BlockUpdatesEnabled = true;
                        var liquidProvider = world.BlockRepository.GetBlockProvider(blockType);
                        liquidProvider.BlockPlaced(new BlockDescriptor { Coordinates = coordinates }, face, world, user);
                    }
                    user.Hotbar[user.SelectedSlot].Item = new ItemStack(BucketItem.ItemID);
                }
            }
        }
    }

    public class LavaBucketItem : BucketItem, IBurnableItem
    {
        public static readonly new short ItemID = 0x147;

        public override short ID { get { return 0x147; } }

        public override string GetDisplayName(short metadata)
        {
            return "Lava Bucket";
        }

        public TimeSpan BurnTime { get { return TimeSpan.FromSeconds(1000); } }

        protected override byte? RelevantBlockType
        {
            get
            {
                return LavaBlock.BlockID;
            }
        }
    }

    public class MilkItem : BucketItem
    {
        public static readonly new short ItemID = 0x14F;

        public override short ID { get { return 0x14F; } }

        public override string GetDisplayName(short metadata)
        {
            return "Milk";
        }

        protected override byte? RelevantBlockType
        {
            get
            {
                return null;
            }
        }
    }

    public class WaterBucketItem : BucketItem
    {
        public static readonly new short ItemID = 0x146;

        public override short ID { get { return 0x146; } }

        public override string GetDisplayName(short metadata)
        {
            return "Water Bucket";
        }

        protected override byte? RelevantBlockType
        {
            get
            {
                return WaterBlock.BlockID;
            }
        }
    }
}