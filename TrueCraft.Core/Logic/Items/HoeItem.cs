using System;
using System.Xml;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Items
{
    public class HoeItem : ToolItem
    {
        public HoeItem(XmlNode node) : base(node)
        {
        }

        public override void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IDimension dimension, IRemoteClient user)
        {
            var id = dimension.GetBlockID(coordinates);
            if (id == DirtBlock.BlockID || id == GrassBlock.BlockID)
            {
                dimension.SetBlockID(coordinates, FarmlandBlock.BlockID);
                dimension.BlockRepository.GetBlockProvider(FarmlandBlock.BlockID).BlockPlaced(
                    new BlockDescriptor { Coordinates = coordinates }, face, dimension, user);
            }
        }
    }
}
