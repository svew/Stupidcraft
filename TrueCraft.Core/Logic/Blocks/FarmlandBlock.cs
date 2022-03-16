using System;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;
using TrueCraft.Core.Server;

namespace TrueCraft.Core.Logic.Blocks
{
    public class FarmlandBlock : BlockProvider
    {
        public enum MoistureLevel : byte
        {
            Dry = 0x0,

            // Any value less than 0x7 is considered 'dry'

            Moist = 0x7
        }

        public static readonly int UpdateIntervalSeconds = 30;

        public static readonly byte BlockID = 0x3C;
        
        public override byte ID { get { return 0x3C; } }
        
        public override double BlastResistance { get { return 3; } }

        public override double Hardness { get { return 0.6; } }

        public override byte Luminance { get { return 0; } }

        public override bool Opaque { get { return true; } }

        public override byte LightOpacity { get { return 255; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Farmland";
        }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Gravel;
            }
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new[] { new ItemStack(DirtBlock.BlockID) };
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(7, 5);
        }

        public bool IsHydrated(GlobalVoxelCoordinates coordinates, IDimension dimension)
        {
            var min = new GlobalVoxelCoordinates(-6 + coordinates.X, coordinates.Y, -6 + coordinates.Z);
            var max = new GlobalVoxelCoordinates(6 + coordinates.X, coordinates.Y + 1, 6 + coordinates.Z);
            for (int x = min.X; x < max.X; x++)
            {
                for (int y = min.Y; y < max.Y; y++) // TODO: This does not check one above the farmland block for some reason
                {
                    for (int z = min.Z; z < max.Z; z++)
                    {
                        var id = dimension.GetBlockID(new GlobalVoxelCoordinates(x, y, z));
                        if (id == WaterBlock.BlockID || id == StationaryWaterBlock.BlockID)
                            return true;
                    }
                }
            }
            return false;
        }

        void HydrationCheckEvent(IMultiplayerServer server, GlobalVoxelCoordinates coords, IDimension dimension)
        {
            if (dimension.GetBlockID(coords) != BlockID)
                return;
            if (MathHelper.Random.Next(3) == 0)
            {
                var meta = dimension.GetMetadata(coords);
                if (IsHydrated(coords, dimension) && meta != 15)
                    meta++;
                else
                {
                    meta--;
                    if (meta == 0)
                    {
                        dimension.SetBlockID(coords, BlockID);
                        return;
                    }
                }
                dimension.SetMetadata(coords, meta);
            }
            var chunk = dimension.FindChunk(coords);
            server.Scheduler.ScheduleEvent("farmland", chunk,
                TimeSpan.FromSeconds(UpdateIntervalSeconds),
                _server => HydrationCheckEvent(_server, coords, dimension));
        }

        public override void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IDimension dimension, IRemoteClient user)
        {
            if (IsHydrated(descriptor.Coordinates, dimension))
            {
                dimension.SetMetadata(descriptor.Coordinates, 1);
            }
            var chunk = dimension.FindChunk(descriptor.Coordinates);
            user.Server.Scheduler.ScheduleEvent("farmland", chunk,
                TimeSpan.FromSeconds(UpdateIntervalSeconds),
                server => HydrationCheckEvent(server, descriptor.Coordinates, dimension));
        }

        public override void BlockLoadedFromChunk(GlobalVoxelCoordinates coords, IMultiplayerServer server, IDimension dimension)
        {
            var chunk = dimension.FindChunk(coords);
            server.Scheduler.ScheduleEvent("farmland", chunk,
                TimeSpan.FromSeconds(UpdateIntervalSeconds),
                s => HydrationCheckEvent(s, coords, dimension));
        }
    }
}