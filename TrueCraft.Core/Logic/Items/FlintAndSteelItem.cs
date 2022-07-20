using System;
using System.Xml;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Items
{
    public class FlintAndSteelItem : ItemProvider, IDurableItem
    {
        public static readonly short ItemID = 0x103;

        public FlintAndSteelItem(XmlNode node) : base(node)
        {
        }

        /// <inheritdoc />
        public short Durability { get { return 65; } }

        public override void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IDimension dimension, IRemoteClient user)
        {
            ServerOnly.Assert();

            coordinates += MathHelper.BlockFaceToCoordinates(face);
            if (dimension.GetBlockID(coordinates) == AirBlock.BlockID)
            {
                dimension.SetBlockID(coordinates, FireBlock.BlockID);
                dimension.BlockRepository.GetBlockProvider(FireBlock.BlockID)
                    .BlockPlaced(dimension.GetBlockData(coordinates), face, dimension, user);

                var slot = user.SelectedItem;
                slot.Metadata += 1;
                if (slot.Metadata >= Durability)
                    slot.Count = 0; // Destroy item
                user.Hotbar[user.SelectedSlot].Item = slot;
            }
        }
    }
}
