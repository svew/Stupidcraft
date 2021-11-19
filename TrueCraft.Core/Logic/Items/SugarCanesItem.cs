using System;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Items
{
    public class SugarCanesItem : ItemProvider
    {
        public static readonly short ItemID = 0x152;

        public override short ID { get { return 0x152; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(11, 1);
        }

        public override string DisplayName { get { return "Sugar Canes"; } }

        public override void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IWorld world, IRemoteClient user)
        {
            coordinates += MathHelper.BlockFaceToCoordinates(face);
            if (SugarcaneBlock.ValidPlacement(new BlockDescriptor { Coordinates = coordinates }, world))
            {
                world.SetBlockID(coordinates, SugarcaneBlock.BlockID);
                item.Count--;
                user.Hotbar[user.SelectedSlot] = item;
                user.Server.BlockRepository.GetBlockProvider(SugarcaneBlock.BlockID).BlockPlaced(
                    new BlockDescriptor { Coordinates = coordinates }, face, world, user);
            }
        }
    }
}