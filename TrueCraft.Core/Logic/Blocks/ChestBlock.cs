using System;
using TrueCraft.Core.World;
using fNbt;
using TrueCraft.Core.Windows;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Server;
using TrueCraft.Core.Inventory;

namespace TrueCraft.Core.Logic.Blocks
{
    public class ChestBlock : BlockProvider, IBurnableItem
    {
        private const int ChestLength = 27;

        public static readonly byte BlockID = 0x36;
        
        public override byte ID { get { return 0x36; } }
        
        public override double BlastResistance { get { return 12.5; } }

        public override double Hardness { get { return 2.5; } }

        public override byte Luminance { get { return 0; } }

        public override bool Opaque { get { return false; } }
        
        public override string DisplayName { get { return "Chest"; } }

        public TimeSpan BurnTime { get { return TimeSpan.FromSeconds(15); } }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Wood;
            }
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(10, 1);
        }

        private static readonly Vector3i[] AdjacentBlocks =
        {
            Vector3i.North,
            Vector3i.South,
            Vector3i.West,
            Vector3i.East
        };

        public override void ItemUsedOnBlock(GlobalVoxelCoordinates coordinates, ItemStack item, BlockFace face, IWorld world, IRemoteClient user)
        {
            int adjacent = 0;
            GlobalVoxelCoordinates coords = coordinates + MathHelper.BlockFaceToCoordinates(face);
            GlobalVoxelCoordinates _ = null;
            // Check for adjacent chests. We can only allow one adjacent check block.
            for (int i = 0; i < AdjacentBlocks.Length; i++)
            {
                if (world.GetBlockID(coords + AdjacentBlocks[i]) == ChestBlock.BlockID)
                {
                    _ = coords + AdjacentBlocks[i];
                    adjacent++;
                }
            }
            if (adjacent <= 1)
            {
                if (!object.ReferenceEquals(_, null))
                {
                    // Confirm that adjacent chest is not a double chest
                    for (int i = 0; i < AdjacentBlocks.Length; i++)
                    {
                        if (world.GetBlockID(_ + AdjacentBlocks[i]) == ChestBlock.BlockID)
                            adjacent++;
                    }
                }
                if (adjacent <= 1)
                    base.ItemUsedOnBlock(coordinates, item, face, world, user);
            }
        }

        public override void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user)
        {
            world.SetMetadata(descriptor.Coordinates, (byte)MathHelper.DirectionByRotationFlat(user.Entity.Yaw, true));
        }

        public override bool BlockRightClicked(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user)
        {
            ServerOnly.Assert();

            GlobalVoxelCoordinates adjacent = null; // No adjacent chest
            GlobalVoxelCoordinates self = descriptor.Coordinates;
            for (int i = 0; i < AdjacentBlocks.Length; i++)
            {
                var test = self + AdjacentBlocks[i];
                if (world.GetBlockID(test) == ChestBlock.BlockID)
                {
                    adjacent = test;
                    var up = world.BlockRepository.GetBlockProvider(world.GetBlockID(test + Vector3i.Up));
                    if (up.Opaque && !(up is WallSignBlock)) // Wall sign blocks are an exception
                        return false; // Obstructed
                    break;
                }
            }
            var upSelf = world.BlockRepository.GetBlockProvider(world.GetBlockID(self + Vector3i.Up));
            if (upSelf.Opaque && !(upSelf is WallSignBlock))
                return false; // Obstructed

            if (!object.ReferenceEquals(adjacent, null))
            {
                // TODO LATER: this assumes that chests cannot be placed next to each other.
                // Ensure that chests are always opened in the same arrangement
                if (adjacent.X < self.X ||
                    adjacent.Z < self.Z)
                {
                    var _ = adjacent;
                    adjacent = self;
                    self = _; // Swap
                }
            }

            IInventoryFactory<IServerSlot> factory = new InventoryFactory<IServerSlot>();
            IItemRepository itemRepository = ItemRepository.Get();
            ISlotFactory<IServerSlot> slotFactory = SlotFactory<IServerSlot>.Get();
            sbyte windowID = WindowIDs.GetWindowID();
            IChestWindow<IServerSlot> window = factory.NewChestWindow(itemRepository,
                slotFactory, windowID, user.Inventory, user.Hotbar,
                world, descriptor.Coordinates, adjacent);

            user.OpenWindow(window);
            return false;
        }

        public override void BlockMined(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user)
        {
            var self = descriptor.Coordinates;
            var entity = world.GetTileEntity(self);
            var manager = user.Server.GetEntityManagerForWorld(world);
            if (entity != null)
            {
                foreach (var item in (NbtList)entity["Items"])
                {
                    var slot = ItemStack.FromNbt((NbtCompound)item);
                    manager.SpawnEntity(new ItemEntity(new Vector3(descriptor.Coordinates.X + 0.5, descriptor.Coordinates.Y + 0.5, descriptor.Coordinates.Z + 0.5), slot));
                }
            }
            world.SetTileEntity(self, null);
            base.BlockMined(descriptor, face, world, user);
        }
    }
}
