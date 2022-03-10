using System;
using TrueCraft.Core.World;
using TrueCraft.Core.Server;
using TrueCraft.Core.Networking;

namespace TrueCraft.Core.Logic.Blocks
{
    public class GrassBlock : BlockProvider
    {
        public static readonly int MinGrowthTime = 60 * 5;
        public static readonly int MaxGrowthTime = 60 * 10;

        static GrassBlock()
        {
            GrowthCandidates = new Vector3i[3 * 3 * 5];
            int i = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    for (int y = -3; y <= 1; y++)
                    {
                        GrowthCandidates[i++] = new Vector3i(x, y, z);
                    }
                }
            }
        }

        private static readonly Vector3i[] GrowthCandidates;

        public static readonly int MaxDecayTime = 60 * 10;
        public static readonly int MinDecayTime = 60 * 2;

        public static readonly byte BlockID = 0x02;
        
        public override byte ID { get { return 0x02; } }
        
        public override double BlastResistance { get { return 3; } }

        public override double Hardness { get { return 0.6; } }

        public override byte Luminance { get { return 0; } }

        public override string GetDisplayName(short metadata)
        {
            return "Grass";
        }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Grass;
            }
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(0, 0);
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new[] { new ItemStack(DirtBlock.BlockID, 1) };
        }

        private void ScheduledUpdate(IDimension world, GlobalVoxelCoordinates coords)
        {
            if (world.IsValidPosition(coords + Vector3i.Up))
            {
                var id = world.GetBlockID(coords + Vector3i.Up);
                var provider = world.BlockRepository.GetBlockProvider(id);
                if (provider.Opaque)
                    world.SetBlockID(coords, DirtBlock.BlockID);
            }
        }

        public override void BlockUpdate(BlockDescriptor descriptor, BlockDescriptor source, IMultiplayerServer server, IDimension world)
        {
            if (source.Coordinates == descriptor.Coordinates + Vector3i.Up)
            {
                var provider = world.BlockRepository.GetBlockProvider(source.ID);
                if (provider.Opaque)
                {
                    var chunk = world.FindChunk(descriptor.Coordinates, generate: false);
                    server.Scheduler.ScheduleEvent("grass", chunk,
                    TimeSpan.FromSeconds(MathHelper.Random.Next(MinDecayTime, MaxDecayTime)), s =>
                    {
                        ScheduledUpdate(world, descriptor.Coordinates);
                    });
                }
            }
        }

        public void TrySpread(GlobalVoxelCoordinates coords, IDimension world, IMultiplayerServer server)
        {
            if (!world.IsValidPosition(coords + Vector3i.Up))
                return;
            var sky = world.GetSkyLight(coords + Vector3i.Up);
            var block = world.GetBlockLight(coords + Vector3i.Up);
            if (sky < 9 && block < 9)
                return;
            for (int i = 0, j = MathHelper.Random.Next(GrowthCandidates.Length); i < GrowthCandidates.Length; i++, j++)
            {
                var candidate = GrowthCandidates[j % GrowthCandidates.Length] + coords;
                if (!world.IsValidPosition(candidate) || !world.IsValidPosition(candidate + Vector3i.Up))
                    continue;
                var id = world.GetBlockID(candidate);
                if (id == DirtBlock.BlockID)
                {
                    var _sky = world.GetSkyLight(candidate + Vector3i.Up);
                    var _block = world.GetBlockLight(candidate + Vector3i.Up);
                    if (_sky < 4 && _block < 4)
                        continue;
                    IChunk chunk;
                    var _candidate = world.FindBlockPosition(candidate, out chunk);
                    bool grow = true;
                    for (int y = candidate.Y; y < chunk.GetHeight((byte)_candidate.X, (byte)_candidate.Z); y++)
                    {
                        var b = world.GetBlockID(new GlobalVoxelCoordinates(candidate.X, y, candidate.Z));
                        var p = world.BlockRepository.GetBlockProvider(b);
                        if (p.LightOpacity >= 2)
                        {
                            grow = false;
                            break;
                        }
                    }
                    if (grow)
                    {
                        world.SetBlockID(candidate, GrassBlock.BlockID);
                        server.Scheduler.ScheduleEvent("grass", chunk,
                            TimeSpan.FromSeconds(MathHelper.Random.Next(MinGrowthTime, MaxGrowthTime)),
                            s => TrySpread(candidate, world, server));
                    }
                    break;
                }
            }
        }

        public override void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IDimension world, IRemoteClient user)
        {
            var chunk = world.FindChunk(descriptor.Coordinates);
            user.Server.Scheduler.ScheduleEvent("grass", chunk,
                TimeSpan.FromSeconds(MathHelper.Random.Next(MinGrowthTime, MaxGrowthTime)),
                s => TrySpread(descriptor.Coordinates, world, user.Server));
        }

        public override void BlockLoadedFromChunk(GlobalVoxelCoordinates coords, IMultiplayerServer server, IDimension world)
        {
            var chunk = world.FindChunk(coords);
            server.Scheduler.ScheduleEvent("grass", chunk,
                TimeSpan.FromSeconds(MathHelper.Random.Next(MinGrowthTime, MaxGrowthTime)),
                s => TrySpread(coords, world, server));
        }
    }
}
