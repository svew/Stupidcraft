using System;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Items
{
    public class SeedsItem : ItemProvider
    {
        public static readonly short ItemID = 0x127;

        public override short ID { get { return 0x127; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(9, 0);
        }

        public override string GetDisplayName(short metadata)
        {
            return "Seeds";
        }

        public override void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IDimension world, IRemoteClient user)
        {
            if (world.GetBlockID(coordinates) == FarmlandBlock.BlockID)
            {
                world.SetBlockID(coordinates + MathHelper.BlockFaceToCoordinates(face), CropsBlock.BlockID);
                world.BlockRepository.GetBlockProvider(CropsBlock.BlockID).BlockPlaced(
                    new BlockDescriptor { Coordinates = coordinates }, face, world, user);
            }
        }
    }
}