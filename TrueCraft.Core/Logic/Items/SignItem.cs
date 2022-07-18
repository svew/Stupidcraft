using System;
using System.Xml;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Items
{
    public class SignItem : ItemProvider
    {
        public static readonly short ItemID = 0x143;

        public SignItem(XmlNode node) : base(node)
        {
        }

        public override void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IDimension dimension, IRemoteClient user)
        {
            if (face == BlockFace.PositiveY)
            {
                var provider = dimension.BlockRepository.GetBlockProvider(UprightSignBlock.BlockID);
                (provider as IItemProvider).ItemUsedOnBlock(coordinates, item, face, dimension, user);
            }
            else
            {
                var provider = dimension.BlockRepository.GetBlockProvider(WallSignBlock.BlockID);
                (provider as IItemProvider).ItemUsedOnBlock(coordinates, item, face, dimension, user);
            }
        }
    }
}