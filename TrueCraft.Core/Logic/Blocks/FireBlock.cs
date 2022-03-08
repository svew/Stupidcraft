using System;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;
using TrueCraft.Core.Server;

namespace TrueCraft.Core.Logic.Blocks
{
    public class FireBlock : BlockProvider
    {
        public static readonly int MinSpreadTime = 1;
        public static readonly int MaxSpreadTime = 5;

        public static readonly byte BlockID = 0x33;
        
        public override byte ID { get { return 0x33; } }
        
        public override double BlastResistance { get { return 0; } }

        public override double Hardness { get { return 0; } }

        public override byte Luminance { get { return 15; } }

        public override bool Opaque { get { return false; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Fire";
        }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Wood; // Yeah, this is what Minecraft actually uses here
            }
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(15, 1);
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new ItemStack[0];
        }

        private static readonly Vector3i[] SpreadableBlocks =
        {
            Vector3i.Down,
            Vector3i.West,
            Vector3i.East,
            Vector3i.North,
            Vector3i.South,
            Vector3i.Up * 1,
            Vector3i.Up * 2,
            Vector3i.Up * 3,
            Vector3i.Up * 4
        };

        private static readonly Vector3i[] AdjacentBlocks =
        {
            Vector3i.Up,
            Vector3i.Down,
            Vector3i.West,
            Vector3i.East,
            Vector3i.North,
            Vector3i.South
        };

        public void DoUpdate(IMultiplayerServer server, IWorld world, BlockDescriptor descriptor)
        {
            var down = descriptor.Coordinates + Vector3i.Down;

            var current = world.GetBlockID(descriptor.Coordinates);
            if (current != FireBlock.BlockID && current != LavaBlock.BlockID && current != StationaryLavaBlock.BlockID)
                return;

            // Decay
            var meta = world.GetMetadata(descriptor.Coordinates);
            meta++;
            if (meta == 0xE)
            {
                if (!world.IsValidPosition(down) || world.GetBlockID(down) != NetherrackBlock.BlockID)
                {
                    world.SetBlockID(descriptor.Coordinates, AirBlock.BlockID);
                    return;
                }
            }
            world.SetMetadata(descriptor.Coordinates, meta);

            if (meta > 9)
            {
                var pick = AdjacentBlocks[meta % AdjacentBlocks.Length];
                var provider = BlockRepository
                    .GetBlockProvider(world.GetBlockID(pick + descriptor.Coordinates));
                if (provider.Flammable)
                    world.SetBlockID(pick + descriptor.Coordinates, AirBlock.BlockID);
            }

            // Spread
            DoSpread(server, world, descriptor);

            // Schedule next event
            ScheduleUpdate(server, world, descriptor);
        }

        public void DoSpread(IMultiplayerServer server, IWorld world, BlockDescriptor descriptor)
        {
            foreach (var coord in SpreadableBlocks)
            {
                var check = descriptor.Coordinates + coord;
                if (world.GetBlockID(check) == AirBlock.BlockID)
                {
                    // Check if this is adjacent to a flammable block
                    foreach (var adj in AdjacentBlocks)
                    {
                        var provider = BlockRepository.GetBlockProvider(
                           world.GetBlockID(check + adj));
                        if (provider.Flammable)
                        {
                            if (provider.Hardness == 0)
                                check = check + adj;

                            // Spread to this block
                            world.SetBlockID(check, FireBlock.BlockID);
                            ScheduleUpdate(server, world, world.GetBlockData(check));
                            break;
                        }
                    }
                }
            }
        }

        public override void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user)
        {
            ScheduleUpdate(user.Server, world, descriptor);
        }

        public void ScheduleUpdate(IMultiplayerServer server, IWorld world, BlockDescriptor descriptor)
        {
            var chunk = world.FindChunk(descriptor.Coordinates);
            server.Scheduler.ScheduleEvent("fire.spread", chunk,
                TimeSpan.FromSeconds(MathHelper.Random.Next(MinSpreadTime, MaxSpreadTime)),
                s => DoUpdate(s, world, descriptor));
        }
    }
}