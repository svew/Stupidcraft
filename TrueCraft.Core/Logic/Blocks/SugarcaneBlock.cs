using System;
using TrueCraft.Core.Logic.Items;
using TrueCraft.Core.World;
using TrueCraft.Core.Server;
using TrueCraft.Core.Networking;

namespace TrueCraft.Core.Logic.Blocks
{
    public class SugarcaneBlock : BlockProvider
    {
        public static readonly int MinGrowthSeconds = 30;
        public static readonly int MaxGrowthSeconds = 120;
        public static readonly int MaxGrowHeight = 3;

        public static readonly byte BlockID = 0x53;
        
        public override byte ID { get { return 0x53; } }
        
        public override double BlastResistance { get { return 0; } }

        public override double Hardness { get { return 0; } }

        public override byte Luminance { get { return 0; } }

        public override bool Opaque { get { return false; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Sugar cane";
        }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Grass;
            }
        }

        public override BoundingBox? BoundingBox
        {
            get
            {
                return null;
            }
        }

        public override BoundingBox? InteractiveBoundingBox
        {
            get
            {
                return new BoundingBox(new Vector3(2 / 16.0, 0, 2 / 16.0), new Vector3(14 / 16.0, 1.0, 14 / 16.0));
            }
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(9, 4);
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new[] { new ItemStack(SugarCanesItem.ItemID) };
        }

        public static bool ValidPlacement(BlockDescriptor descriptor, IDimension world)
        {
            var below = world.GetBlockID(descriptor.Coordinates + Vector3i.Down);
            if (below != SugarcaneBlock.BlockID && below != GrassBlock.BlockID && below != DirtBlock.BlockID)
                return false;
            var toCheck = new[]
            {
                Vector3i.Down + Vector3i.West,
                Vector3i.Down + Vector3i.East,
                Vector3i.Down + Vector3i.North,
                Vector3i.Down + Vector3i.South
            };
            if (below != BlockID)
            {
                bool foundWater = false;
                for (int i = 0; i < toCheck.Length; i++)
                {
                    var id = world.GetBlockID(descriptor.Coordinates + toCheck[i]);
                    if (id == WaterBlock.BlockID || id == StationaryWaterBlock.BlockID)
                    {
                        foundWater = true;
                        break;
                    }
                }
                return foundWater;
            }
            return true;
        }

        public override void BlockUpdate(BlockDescriptor descriptor, BlockDescriptor source, IMultiplayerServer server, IDimension world)
        {
            if (!ValidPlacement(descriptor, world))
            {
                // Destroy self
                world.SetBlockID(descriptor.Coordinates, 0);
                GenerateDropEntity(descriptor, world, server, ItemStack.EmptyStack);
            }
        }

        private void TryGrowth(IMultiplayerServer server, GlobalVoxelCoordinates coords, IDimension world)
        {
            if (world.GetBlockID(coords) != BlockID)
                return;
            // Find current height of stalk
            int height = 0;
            for (int y = -MaxGrowHeight; y <= MaxGrowHeight; y++)
            {
                if (world.GetBlockID(coords + (Vector3i.Down * y)) == BlockID)
                    height++;
            }
            if (height < MaxGrowHeight)
            {
                var meta = world.GetMetadata(coords);
                meta++;
                world.SetMetadata(coords, meta);
                var chunk = world.FindChunk(coords);
                if (meta == 15)
                {
                    if (world.GetBlockID(coords + Vector3i.Up) == 0)
                    {
                        world.SetBlockID(coords + Vector3i.Up, BlockID);
                        server.Scheduler.ScheduleEvent("sugarcane", chunk,
                            TimeSpan.FromSeconds(MathHelper.Random.Next(MinGrowthSeconds, MaxGrowthSeconds)),
                            (_server) => TryGrowth(_server, coords + Vector3i.Up, world));
                    }
                }
                else
                {
                    server.Scheduler.ScheduleEvent("sugarcane", chunk,
                        TimeSpan.FromSeconds(MathHelper.Random.Next(MinGrowthSeconds, MaxGrowthSeconds)),
                        (_server) => TryGrowth(_server, coords, world));
                }
            }
        }

        public override void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IDimension world, IRemoteClient user)
        {
            var chunk = world.FindChunk(descriptor.Coordinates);
            user.Server.Scheduler.ScheduleEvent("sugarcane", chunk,
                TimeSpan.FromSeconds(MathHelper.Random.Next(MinGrowthSeconds, MaxGrowthSeconds)),
                (server) => TryGrowth(server, descriptor.Coordinates, world));
        }

        public override void BlockLoadedFromChunk(GlobalVoxelCoordinates coords, IMultiplayerServer server, IDimension world)
        {
            var chunk = world.FindChunk(coords);
            server.Scheduler.ScheduleEvent("sugarcane", chunk,
                TimeSpan.FromSeconds(MathHelper.Random.Next(MinGrowthSeconds, MaxGrowthSeconds)),
                s => TryGrowth(s, coords, world));
        }
    }
}