using System;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Items
{
    public class RedstoneItem : ItemProvider
    {
        public static readonly short ItemID = 0x14B;

        public override short ID { get { return 0x14B; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(8, 3);
        }

        public override string DisplayName { get { return "Redstone"; } }

        public override void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IWorld world, IRemoteClient user)
        {
            ServerOnly.Assert();

            coordinates += MathHelper.BlockFaceToCoordinates(face);
            IBlockProvider supportingBlock = world.BlockRepository.GetBlockProvider(world.GetBlockID(coordinates + Vector3i.Down));

            if (supportingBlock.Opaque)
            {
                world.SetBlockID(coordinates, RedstoneDustBlock.BlockID);
                item.Count--;
                user.Hotbar[user.SelectedSlot].Item = item;
            }
        }
    }
}