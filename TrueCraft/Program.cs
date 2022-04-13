using System;
using System.IO;
using System.Net;
using System.Threading;
using TrueCraft.Core.Logging;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;
using TrueCraft.Commands;
using TrueCraft.Core;
using TrueCraft.Profiling;
using TrueCraft.World;
using System.Collections.Generic;
using TrueCraft.Core.Logic;

namespace TrueCraft
{
    public class Program
    {
        public static ServerConfiguration ServerConfiguration;

        public static MultiplayerServer Server;

        public static void Main(string[] args)
        {
            // TODO: World path must be passed here.
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
                world = TrueCraft.World.World.LoadWorld("world");
            }
            catch(DirectoryNotFoundException)
            {
                int seed = MathHelper.Random.Next();
                IDimensionFactory factory = new DimensionFactory();
                world = new TrueCraft.World.World(seed, Paths.Worlds, "world", factory,
                    new PanDimensionalVoxelCoordinates(DimensionID.Overworld, 0, 0, 0));
                world.Save();

                IDimensionServer overWorld = (IDimensionServer)world[DimensionID.Overworld];
                overWorld.Initialize(new GlobalChunkCoordinates(0, 0), Server, null);

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
            foreach (var w in Server.World)
                ((IDimensionServer)w).Save();
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
