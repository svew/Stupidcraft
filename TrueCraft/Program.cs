using System;
using System.IO;
using System.Net;
using System.Threading;
using TrueCraft.Core.Logging;
using TrueCraft.Core.Server;
using TrueCraft.Core.TerrainGen;
using TrueCraft.Core.World;
using TrueCraft.Commands;
using TrueCraft.Core;
using TrueCraft.Profiling;

namespace TrueCraft
{
    public class Program
    {
        public static ServerConfiguration ServerConfiguration;

        public static MultiplayerServer Server;

        public static void Main(string[] args)
        {
            Server = MultiplayerServer.Get();

            Server.AddLogProvider(new ConsoleLogProvider(LogCategory.Notice | LogCategory.Warning | LogCategory.Error | LogCategory.Debug));
#if DEBUG
            Server.AddLogProvider(new FileLogProvider(new StreamWriter("packets.log", false), LogCategory.Packets));
#endif

            ServerConfiguration = Configuration.LoadConfiguration<ServerConfiguration>("config.yaml");

            var buckets = ServerConfiguration.Debug?.Profiler?.Buckets?.Split(',');
            if (buckets != null)
            {
                foreach (var bucket in buckets)
                {
                    Profiler.EnableBucket(bucket.Trim());
                }
            }

            if (ServerConfiguration.Debug.DeleteWorldOnStartup)
            {
                if (Directory.Exists("world"))
                    Directory.Delete("world", true);
            }
            if (ServerConfiguration.Debug.DeletePlayersOnStartup)
            {
                if (Directory.Exists("players"))
                    Directory.Delete("players", true);
            }
            IWorld world;
            try
            {
                world = World.LoadWorld("world");
                Server.AddWorld(world);
            }
            catch
            {
                world = new World("default", new StandardGenerator());
                world.BlockRepository = Server.BlockRepository;
                world.Save("world");
                Server.AddWorld(world);
                Server.Log(LogCategory.Notice, "Generating world around spawn point...");
                for (int x = -5; x < 5; x++)
                {
                    for (int z = -5; z < 5; z++)
                        world.GetChunk(new GlobalChunkCoordinates(x, z));
                    int progress = (int)(((x + 5) / 10.0) * 100);
                    if (progress % 10 == 0)
                        Server.Log(LogCategory.Notice, "{0}% complete", progress + 10);
                }
                Server.Log(LogCategory.Notice, "Simulating the world for a moment...");
                for (int x = -5; x < 5; x++)
                {
                    for (int z = -5; z < 5; z++)
                    {
                        var chunk = world.GetChunk(new GlobalChunkCoordinates(x, z));
                        for (byte _x = 0; _x < Chunk.Width; _x++)
                        {
                            for (byte _z = 0; _z < Chunk.Depth; _z++)
                            {
                                for (int _y = 0; _y < chunk.GetHeight(_x, _z); _y++)
                                {
                                    GlobalVoxelCoordinates coords = new GlobalVoxelCoordinates(x + _x, _y, z + _z);
                                    var data = world.GetBlockData(coords);
                                    var provider = world.BlockRepository.GetBlockProvider(data.ID);
                                    provider.BlockUpdate(data, data, Server, world);
                                }
                            }
                        }
                    }
                    int progress = (int)(((x + 5) / 10.0) * 100);
                    if (progress % 10 == 0)
                        Server.Log(LogCategory.Notice, "{0}% complete", progress + 10);
                }
                Server.Log(LogCategory.Notice, "Lighting the world (this will take a moment)...");
                foreach (var lighter in Server.WorldLighters)
                {
                    while (lighter.TryLightNext()) ;
                }
            }
            world.Save();

            Server.Start(new IPEndPoint(IPAddress.Parse(ServerConfiguration.ServerAddress), ServerConfiguration.ServerPort));
            Console.CancelKeyPress += HandleCancelKeyPress;
            Server.Scheduler.ScheduleEvent("world.save", null,
                TimeSpan.FromSeconds(ServerConfiguration.WorldSaveInterval), SaveWorlds);
            while (true)
            {
                Thread.Yield();
            }
        }

        static void SaveWorlds(IMultiplayerServer server)
        {
            Server.Log(LogCategory.Notice, "Saving world...");
            foreach (var w in Server.Worlds)
                w.Save();
            Server.Log(LogCategory.Notice, "Done.");
            server.Scheduler.ScheduleEvent("world.save", null,
                TimeSpan.FromSeconds(ServerConfiguration.WorldSaveInterval), SaveWorlds);
        }

        static void HandleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Server.Stop();
        }
    }
}
