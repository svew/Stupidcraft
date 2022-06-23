using System;
using TrueCraft.Core.World;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Server;
using TrueCraft.Core.Networking;

namespace TrueCraft.Core.Logic.Blocks
{
    public class CactusBlock : BlockProvider
    {
        public static readonly int MinGrowthSeconds = 30;
        public static readonly int MaxGrowthSeconds = 60;
        public static readonly int MaxGrowHeight = 3;

        public static readonly byte BlockID = 0x51;
        
        public override byte ID { get { return 0x51; } }
        
        public override double BlastResistance { get { return 2; } }

        public override double Hardness { get { return 0.4; } }

        public override byte Luminance { get { return 0; } }

        public override bool Opaque { get { return false; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Cactus";
        }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Cloth;
            }
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(6, 4);
        }

        public bool ValidCactusPosition(BlockDescriptor descriptor, IBlockRepository repository, IDimension dimension, bool checkNeighbor = true, bool checkSupport = true)
        {
            if (checkNeighbor)
            {
                GlobalVoxelCoordinates coords = descriptor.Coordinates;
                foreach (Vector3i neighbor in Vector3i.Neighbors4)
                    if (dimension.GetBlockID(coords + neighbor) != AirBlock.BlockID)
                        return false;
            }

            if (checkSupport)
            {
                var supportingBlock = repository.GetBlockProvider(dimension.GetBlockID(descriptor.Coordinates + Vector3i.Down));
                if ((supportingBlock.ID != CactusBlock.BlockID) && (supportingBlock.ID != SandBlock.BlockID))
                    return false;
            }

            return true;
        }

        private void TryGrowth(IMultiplayerServer server, GlobalVoxelCoordinates coords, IDimension dimension)
        {
            IChunk? chunk = dimension.GetChunk(coords);
            if (chunk is null || dimension.GetBlockID(coords) != BlockID)
                return;
            // Find current height of stalk
            int height = 0;
            for (int y = -MaxGrowHeight; y <= MaxGrowHeight; y++)
            {
                if (dimension.GetBlockID(coords + (Vector3i.Down * y)) == BlockID)
                    height++;
            }
            if (height < MaxGrowHeight)
            {
                var meta = dimension.GetMetadata(coords);
                meta++;
                dimension.SetMetadata(coords, meta);
                if (meta == 15)
                {
                    if (dimension.GetBlockID(coords + Vector3i.Up) == 0)
                    {
                        dimension.SetBlockID(coords + Vector3i.Up, BlockID);
                        server.Scheduler.ScheduleEvent("cactus", chunk,
                            TimeSpan.FromSeconds(MathHelper.Random.Next(MinGrowthSeconds, MaxGrowthSeconds)),
                            (_server) => TryGrowth(_server, coords + Vector3i.Up, dimension));
                    }
                }
                else
                {
                    server.Scheduler.ScheduleEvent("cactus", chunk,
                        TimeSpan.FromSeconds(MathHelper.Random.Next(MinGrowthSeconds, MaxGrowthSeconds)),
                        (_server) => TryGrowth(_server, coords, dimension));
                }
            }
        }

        public void DestroyCactus(BlockDescriptor descriptor, IMultiplayerServer server, IDimension dimension)
        {
            ServerOnly.Assert();

            var toDrop = 0;

            // Search upwards
            for (int y = descriptor.Coordinates.Y; y < 127; y++)
            {
                var coordinates = new GlobalVoxelCoordinates(descriptor.Coordinates.X, y, descriptor.Coordinates.Z);
                if (dimension.GetBlockID(coordinates) == CactusBlock.BlockID)
                {
                    dimension.SetBlockID(coordinates, AirBlock.BlockID);
                    toDrop++;
                }
            }

            // Search downwards.
            for (int y = descriptor.Coordinates.Y - 1; y > 0; y--)
            {
                var coordinates = new GlobalVoxelCoordinates(descriptor.Coordinates.X, y, descriptor.Coordinates.Z);
                if (dimension.GetBlockID(coordinates) == CactusBlock.BlockID)
                {
                    dimension.SetBlockID(coordinates, AirBlock.BlockID);
                    toDrop++;
                }
            }

            IEntityManager manager = ((IDimensionServer)dimension).EntityManager;
            manager.SpawnEntity(
                new ItemEntity((Vector3)(descriptor.Coordinates + Vector3i.Up),
                    new ItemStack(CactusBlock.BlockID, (sbyte)toDrop)));
        }

        public override void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IDimension dimension, IRemoteClient user)
        {
            ServerOnly.Assert();

            if (ValidCactusPosition(descriptor, dimension.BlockRepository, dimension))
                base.BlockPlaced(descriptor, face, dimension, user);
            else
            {
                dimension.SetBlockID(descriptor.Coordinates, AirBlock.BlockID);

                IEntityManager manager = ((IDimensionServer)dimension).EntityManager;
                manager.SpawnEntity(
                    new ItemEntity((Vector3)(descriptor.Coordinates + Vector3i.Up),
                        new ItemStack(CactusBlock.BlockID, (sbyte)1)));
                // user.Inventory.PickUpStack() wasn't working?
            }

            IChunk chunk = dimension.GetChunk(descriptor.Coordinates)!;
            user.Server.Scheduler.ScheduleEvent("cactus", chunk,
                TimeSpan.FromSeconds(MathHelper.Random.Next(MinGrowthSeconds, MaxGrowthSeconds)),
                (server) => TryGrowth(server, descriptor.Coordinates, dimension));
        }

        public override void BlockUpdate(BlockDescriptor descriptor, BlockDescriptor source, IMultiplayerServer server, IDimension dimension)
        {
            if (!ValidCactusPosition(descriptor, dimension.BlockRepository, dimension))
                DestroyCactus(descriptor, server, dimension);
            base.BlockUpdate(descriptor, source, server, dimension);
        }

        public override void BlockLoadedFromChunk(GlobalVoxelCoordinates coords, IMultiplayerServer server, IDimension dimension)
        {
            var chunk = dimension.GetChunk(coords);
            server.Scheduler.ScheduleEvent("cactus", chunk,
                TimeSpan.FromSeconds(MathHelper.Random.Next(MinGrowthSeconds, MaxGrowthSeconds)),
                s => TryGrowth(s, coords, dimension));
        }
    }
}