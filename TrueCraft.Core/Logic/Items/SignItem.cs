using System;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Items
{
    public class SignItem : ItemProvider
    {
        public static readonly short ItemID = 0x143;

        public override short ID { get { return 0x143; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(10, 2);
        }

        public override sbyte MaximumStack { get { return 1; } }

        public override string GetDisplayName(short metadata)
        {
            return "Sign";
        }

        public override void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IDimension dimension, IRemoteClient user)
        {
            if (face == BlockFace.PositiveY)
            {
                var provider = user.Server.BlockRepository.GetBlockProvider(UprightSignBlock.BlockID);
                (provider as IItemProvider).ItemUsedOnBlock(coordinates, item, face, dimension, user);
            }
            else
            {
                var provider = user.Server.BlockRepository.GetBlockProvider(WallSignBlock.BlockID);
                (provider as IItemProvider).ItemUsedOnBlock(coordinates, item, face, dimension, user);
            }
        }
    }
}