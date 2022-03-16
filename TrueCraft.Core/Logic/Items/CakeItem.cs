using System;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Items
{
    public class CakeItem : FoodItem // TODO: This isn't really a FoodItem
    {
        public static readonly short ItemID = 0x162;

        public override short ID { get { return 0x162; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(13, 1);
        }

        //This is per "slice"
        public override float Restores { get { return 1.5f; } }

        public override string GetDisplayName(short metadata)
        {
            return "Cake";
        }

        public override void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IDimension dimension, IRemoteClient user)
        {
            ServerOnly.Assert();

            coordinates += MathHelper.BlockFaceToCoordinates(face);
            var old = dimension.BlockRepository.GetBlockProvider(dimension.GetBlockID(coordinates));
            if (old.Hardness == 0)
            {
                dimension.SetBlockID(coordinates, CakeBlock.BlockID);
                item.Count--;
                user.Hotbar[user.SelectedSlot].Item = item;
            }
        }
    }
}