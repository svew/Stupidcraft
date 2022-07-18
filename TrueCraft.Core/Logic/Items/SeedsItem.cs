using System;
using System.Xml;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Items
{
    public class SeedsItem : ItemProvider
    {
        public static readonly short ItemID = 0x127;

        public SeedsItem(XmlNode node) : base(node)
        {
        }

        public override void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IDimension dimension, IRemoteClient user)
        {
            if (dimension.GetBlockID(coordinates) == FarmlandBlock.BlockID)
            {
                dimension.SetBlockID(coordinates + MathHelper.BlockFaceToCoordinates(face), CropsBlock.BlockID);
                dimension.BlockRepository.GetBlockProvider(CropsBlock.BlockID).BlockPlaced(
                    new BlockDescriptor { Coordinates = coordinates }, face, dimension, user);
            }
        }
    }
}