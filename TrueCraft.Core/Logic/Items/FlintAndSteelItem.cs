using System;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Items
{
    public class FlintAndSteelItem : ToolItem
    {
        public static readonly short ItemID = 0x103;
        public override short ID { get { return 0x103; } }
        public override sbyte MaximumStack { get { return 1; } }
        public override short BaseDurability { get { return 65; } }
        public override string DisplayName { get { return "Flint and Steel"; } }

        public override Tuple<int, int> GetIconTexture(byte metadata)
        {
            return new Tuple<int, int>(5, 0);
        }

        public override void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IWorld world, IRemoteClient user)
        {
            ServerOnly.Assert();

            coordinates += MathHelper.BlockFaceToCoordinates(face);
            if (world.GetBlockID(coordinates) == AirBlock.BlockID)
            {
                world.SetBlockID(coordinates, FireBlock.BlockID);
                world.BlockRepository.GetBlockProvider(FireBlock.BlockID)
                    .BlockPlaced(world.GetBlockData(coordinates), face, world, user);

                var slot = user.SelectedItem;
                slot.Metadata += 1;
                if (slot.Metadata >= Uses)
                    slot.Count = 0; // Destroy item
                user.Hotbar[user.SelectedSlot].Item = slot;
            }
        }
    }
}
