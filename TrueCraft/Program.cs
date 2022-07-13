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
using TrueCraft.Core.Lighting;
using TrueCraft.Handlers;

namespace TrueCraft
{
    public class Program
    {
        public static ServerConfiguration? ServerConfiguration;

        public static MultiplayerServer? Server;

        public static IServerServiceLocator ServiceLocator = null!;

        public static void Main(string[] args)
        {
            try
            {
                IServiceLocator coreServiceLocator = Discover.DoDiscovery(new Discover());

                // TODO: World path must be passed here.
                Server = new MultiplayerServer(coreServiceLocator);

                ServiceLocator = new ServerServiceLocator(Server, coreServiceLocator);
                InteractionHandlers.ServiceLocator = ServiceLocator;

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

                if (ServerConfiguration.Debug!.DeleteWorldOnStartup)
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
                if (!Directory.Exists("world"))
                {
                    int seed = MathHelper.Random.Next();
                    TrueCraft.World.World.CreateWorld(seed, Paths.Worlds, "world");
                }

                world = TrueCraft.World.World.LoadWorld(ServiceLocator, "world");
                ServiceLocator.World = world;
                Server.World = world;

                IDimensionServer overWorld = (IDimensionServer)world[DimensionID.Overworld];
                overWorld.Initialize(new GlobalChunkCoordinates(0, 0), Server, null);

                // TODO: Is this needed when loading (and not creating)?
                Server.Log(LogCategory.Notice, "Lighting the world (this will take a moment)...");
                foreach (Lighting lighter in Server.WorldLighters)
                {
                    while (lighter.TryLightNext()) ;
                }

                Server.Start(new IPEndPoint(IPAddress.Parse(ServerConfiguration.ServerAddress), ServerConfiguration.ServerPort));
                Console.CancelKeyPress += HandleCancelKeyPress;
                Server.Scheduler.ScheduleEvent("world.save", null,
                    TimeSpan.FromSeconds(ServerConfiguration.WorldSaveInterval), SaveWorlds);
                while (true)
                {
                    Thread.Yield();
                }
            }
            catch (Exception ex)
            {
                if (Server is not null)
                {
                    Server.Log(LogCategory.Error, ex.Message);
                    SaveWorlds(Server);
                }
            }
        }

        static void SaveWorlds(IMultiplayerServer server)
        {
            // TODO: Surely, this is too time consuming of an operation to be done from
            //       a scheduled event.
            Server!.Log(LogCategory.Notice, "Saving world...");
            ((IWorld?)Server.World)?.Save();
            Server.Log(LogCategory.Notice, "Done.");
            server.Scheduler.ScheduleEvent("world.save", null,
                TimeSpan.FromSeconds(ServerConfiguration!.WorldSaveInterval), SaveWorlds);
        }

        static void HandleCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            Server!.Stop();
        }
    }
}
