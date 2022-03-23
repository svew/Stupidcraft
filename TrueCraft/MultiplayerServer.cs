using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TrueCraft.Core;
using TrueCraft.Core.Logging;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Server;
using TrueCraft.Core.Lighting;
using TrueCraft.Core.World;
using TrueCraft.Profiling;
using TrueCraft.Commands;

namespace TrueCraft
{
    public class MultiplayerServer : IMultiplayerServer, IDisposable
    {
        private static MultiplayerServer _singleton = null;

        public event EventHandler<ChatMessageEventArgs> ChatMessageReceived;
        public event EventHandler<PlayerJoinedQuitEventArgs> PlayerJoined;
        public event EventHandler<PlayerJoinedQuitEventArgs> PlayerQuit;

        public IAccessConfiguration AccessConfiguration { get; internal set; }

        public IPacketReader PacketReader { get; private set; }
        public IList<IRemoteClient> Clients { get; private set; }
        public IList<IDimension> Dimensions { get; private set; }
        public IList<IEntityManager> EntityManagers { get; private set; }
        public IList<Lighting> WorldLighters { get; set; }
        public IEventScheduler Scheduler { get; private set; }
        public IBlockRepository BlockRepository { get; private set; }
        public IItemRepository ItemRepository { get; private set; }

        public bool EnableClientLogging { get; set; }
        public IPEndPoint EndPoint { get; private set; }

        private static readonly int MillisecondsPerTick = 1000 / 20;

        private bool _BlockUpdatesEnabled = true;

        private struct BlockUpdate
        {
            public GlobalVoxelCoordinates Coordinates;
            public IDimension Dimension;
        }
        private Queue<BlockUpdate> PendingBlockUpdates { get; set; }
        public bool BlockUpdatesEnabled
        {
            get
            {
                return _BlockUpdatesEnabled;
            }
            set
            {
                _BlockUpdatesEnabled = value;
                if (_BlockUpdatesEnabled)
                {
                    ProcessBlockUpdates();
                }
            }
        }

        private Thread _masterTick;
        private List<AutoResetEvent> _lstAutoResetEvents;
        private CountdownEvent _cde;
        private Thread _environmentWorker;
        private AutoResetEvent _environmentAutoReset;



        private TcpListener Listener;
        private readonly PacketHandler[] PacketHandlers;
        private IList<ILogProvider> LogProviders;
        private Stopwatch Time;
        private ConcurrentBag<Tuple<IDimension, IChunk>> ChunksToSchedule;
        internal object ClientLock = new object();
        
        private QueryProtocol QueryProtocol;

        internal bool ShuttingDown { get; private set; }
        
        private MultiplayerServer()
        {
            TrueCraft.Core.WhoAmI.Answer = Core.IAm.Server;
            TrueCraft.Core.Inventory.InventoryFactory<IServerSlot>.RegisterInventoryFactory(new TrueCraft.Inventory.InventoryFactory());
            TrueCraft.Core.Inventory.SlotFactory<IServerSlot>.RegisterSlotFactory(new TrueCraft.Inventory.SlotFactory());
            var reader = new PacketReader();
            PacketReader = reader;
            Clients = new List<IRemoteClient>();

            // Initialize threads
            _masterTick = new Thread(MasterTick);
            _lstAutoResetEvents = new List<AutoResetEvent>(1);
            _environmentWorker = new Thread(TickedThreadEntry);
            _environmentAutoReset = new AutoResetEvent(false);
            _lstAutoResetEvents.Add(_environmentAutoReset);
            _cde = new CountdownEvent(_lstAutoResetEvents.Count);

            PacketHandlers = new PacketHandler[0x100];
            Dimensions = new List<IDimension>();
            EntityManagers = new List<IEntityManager>();
            LogProviders = new List<ILogProvider>();
            Scheduler = new EventScheduler(this);

            Discover.DoDiscovery(new Discover());

            BlockRepository = TrueCraft.Core.Logic.BlockRepository.Get();

            ItemRepository = TrueCraft.Core.Logic.ItemRepository.Get();

            BlockProvider.BlockRepository = BlockRepository;

            PendingBlockUpdates = new Queue<BlockUpdate>();
            EnableClientLogging = false;
            QueryProtocol = new TrueCraft.QueryProtocol(this);
            WorldLighters = new List<Lighting>();
            ChunksToSchedule = new ConcurrentBag<Tuple<IDimension, IChunk>>();
            Time = new Stopwatch();

            AccessConfiguration = Configuration.LoadConfiguration<AccessConfiguration>("access.yaml");

            reader.RegisterCorePackets();
            Handlers.PacketHandlers.RegisterHandlers(this);
        }

        public static MultiplayerServer Get()
        {
            if (_singleton == null)
                _singleton = new MultiplayerServer();

            return _singleton;
        }

        private long lastTick = 0;

        private void MasterTick()
        {
            while (!ShuttingDown)
            {
                long start = Time.ElapsedMilliseconds;

                // Release all threads for this tick
                foreach (AutoResetEvent j in _lstAutoResetEvents)
                    j.Set();

                // Wait for all threads to confirm they have started
                _cde.Wait();
                _cde.Reset();

                long end = Time.ElapsedMilliseconds;
                int wait = (int)Math.Max(MillisecondsPerTick - (end - start), 0);
                Thread.Sleep(wait);
            }
        }

        public void RegisterPacketHandler(byte packetId, PacketHandler handler)
        {
            PacketHandlers[packetId] = handler;
        }

        public void Start(IPEndPoint endPoint)
        {
            Scheduler.DisabledEvents.Clear();
            if (Program.ServerConfiguration.DisabledEvents != null)
                Program.ServerConfiguration.DisabledEvents.ToList().ForEach(
                    ev => Scheduler.DisabledEvents.Add(ev));
            ShuttingDown = false;
            Time.Reset();
            Time.Start();
            Listener = new TcpListener(endPoint);
            Listener.Start();
            EndPoint = (IPEndPoint)Listener.LocalEndpoint;

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += AcceptClient;

            if (!Listener.Server.AcceptAsync(args))
                AcceptClient(this, args);
            
            Log(LogCategory.Notice, "Running TrueCraft server on {0}", EndPoint);

            // Start all threads
            _environmentWorker.Start(new Tuple<Action, AutoResetEvent>(DoEnvironment, _environmentAutoReset));
            _masterTick.Start();

            if(Program.ServerConfiguration.Query)
                QueryProtocol.Start();
        }

        public void Stop()
        {
            ShuttingDown = true;
            Listener.Stop();
            if(Program.ServerConfiguration.Query)
                QueryProtocol.Stop();
            foreach (var w in Dimensions)
                w.Save();

            // NOTE: DisconnectClient modifies the Clients collection!
            for (int j = Clients.Count - 1; j >= 0; j --)
                DisconnectClient(Clients[j]);
        }

        public void AddDimension(IDimension dimension)
        {
            Dimensions.Add(dimension);

            dimension.ChunkGenerated += HandleChunkGenerated;
            dimension.ChunkLoaded += HandleChunkLoaded;
            dimension.BlockChanged += HandleBlockChanged;
            var manager = new EntityManager(this, dimension);
            EntityManagers.Add(manager);
            var lighter = new Lighting(dimension, BlockRepository);
            WorldLighters.Add(lighter);
            foreach (var chunk in dimension)
                HandleChunkLoaded(dimension, new ChunkLoadedEventArgs(chunk));
        }

        void HandleChunkLoaded(object sender, ChunkLoadedEventArgs e)
        {
            if (Program.ServerConfiguration.EnableEventLoading)
                ChunksToSchedule.Add(new Tuple<IDimension, IChunk>(sender as IDimension, e.Chunk));
            if (Program.ServerConfiguration.EnableLighting)
            {
                var lighter = WorldLighters.SingleOrDefault(l => l.Dimension == sender);
                lighter.InitialLighting(e.Chunk, false);
            }
        }

        void HandleBlockChanged(object sender, BlockChangeEventArgs e)
        {
            // TODO: Propegate lighting changes to client (not possible with beta 1.7.3 protocol)
            if (e.NewBlock.ID != e.OldBlock.ID || e.NewBlock.Metadata != e.OldBlock.Metadata)
            {
                for (int i = 0, ClientsCount = Clients.Count; i < ClientsCount; i++)
                {
                    var client = (RemoteClient)Clients[i];
                    // TODO: Confirm that the client knows of this block
                    if (client.LoggedIn && client.Dimension == sender)
                    {
                        client.QueuePacket(new BlockChangePacket(e.Position.X, (sbyte)e.Position.Y, e.Position.Z,
                                (sbyte)e.NewBlock.ID, (sbyte)e.NewBlock.Metadata));
                    }
                }
                PendingBlockUpdates.Enqueue(new BlockUpdate { Coordinates = e.Position, Dimension = sender as IDimension });
                ProcessBlockUpdates();
                if (Program.ServerConfiguration.EnableLighting)
                {
                    var lighter = WorldLighters.SingleOrDefault(l => l.Dimension == sender);
                    if (lighter != null)
                    {
                        Vector3 posA = new Vector3(e.Position.X, 0, e.Position.Z);
                        Vector3 posB = new Vector3(e.Position.X + 1, Dimension.Height, e.Position.Z + 1);
                        lighter.EnqueueOperation(new BoundingBox(posA, posB), true);
                        lighter.EnqueueOperation(new BoundingBox(posA, posB), false);
                    }
                }
            }
        }

        void HandleChunkGenerated(object sender, ChunkLoadedEventArgs e)
        {
            if (Program.ServerConfiguration.EnableLighting)
            {
                var lighter = new Lighting(sender as IDimension, BlockRepository);
                lighter.InitialLighting(e.Chunk, false);
            }
            else
            {
                for (int i = 0; i < e.Chunk.SkyLight.Length * 2; i++)
                {
                    e.Chunk.SkyLight[i] = 0xF;
                }
            }
            HandleChunkLoaded(sender, e);
        }

        void ScheduleUpdatesForChunk(IDimension dimension, IChunk chunk)
        {
            chunk.UpdateHeightMap();
            int _x = chunk.Coordinates.X * WorldConstants.ChunkWidth;
            int _z = chunk.Coordinates.Z * WorldConstants.ChunkDepth;
            LocalVoxelCoordinates _coords;
            GlobalVoxelCoordinates coords;
            for (byte x = 0; x < WorldConstants.ChunkWidth; x++)
            {
                for (byte z = 0; z < WorldConstants.ChunkDepth; z++)
                {
                    for (int y = 0; y <= chunk.GetHeight(x, z); y++)
                    {
                        _coords = new LocalVoxelCoordinates(x, y, z);
                        var id = chunk.GetBlockID(_coords);
                        if (id == 0)
                            continue;
                        coords = new GlobalVoxelCoordinates(_x + x, y, _z + z);
                        var provider = BlockRepository.GetBlockProvider(id);
                        provider.BlockLoadedFromChunk(coords, this, dimension);
                    }
                }
            }
        }

        private void ProcessBlockUpdates()
        {
            if (!BlockUpdatesEnabled)
                return;

            while (PendingBlockUpdates.Count != 0)
            {
                var update = PendingBlockUpdates.Dequeue();
                var source = update.Dimension.GetBlockData(update.Coordinates);
                foreach (var offset in Vector3i.Neighbors6)
                {
                    var descriptor = update.Dimension.GetBlockData(update.Coordinates + offset);
                    var provider = BlockRepository.GetBlockProvider(descriptor.ID);
                    if (provider != null)
                        provider.BlockUpdate(descriptor, source, this, update.Dimension);
                }
            }
        }

        public void AddLogProvider(ILogProvider provider)
        {
            LogProviders.Add(provider);
        }

        public void Log(LogCategory category, string text, params object[] parameters)
        {
            for (int i = 0, LogProvidersCount = LogProviders.Count; i < LogProvidersCount; i++)
            {
                var provider = LogProviders[i];
                provider.Log(category, text, parameters);
            }
        }

        public IEntityManager GetEntityManagerForWorld(IDimension dimension)
        {
            for (int i = 0; i < EntityManagers.Count; i++)
            {
                var manager = EntityManagers[i] as EntityManager;
                if (manager.Dimension == dimension)
                    return manager;
            }
            return null;
        }

        public void SendMessage(string message, params object[] parameters)
        {
            var compiled = string.Format(message, parameters);
            var parts = compiled.Split('\n');
            foreach (var client in Clients)
            {
                foreach (var part in parts)
                    client.SendMessage(part);
            }
            Log(LogCategory.Notice, ChatColor.RemoveColors(compiled));
        }

        protected internal void OnChatMessageReceived(ChatMessageEventArgs e)
        {
            HandleChatMessageReceived(e);
            if (ChatMessageReceived != null)
                ChatMessageReceived(this, e);
        }

        private void HandleChatMessageReceived(ChatMessageEventArgs e)
        {
            var message = e.Message;

            if (!message.StartsWith("/") || message.StartsWith("//"))
                SendChatMessage(e.Client.Username, message);
            else
                e.PreventDefault = ProcessChatCommand(e);
        }

        private void SendChatMessage(string username, string message)
        {
            if (message.StartsWith("//"))
                message = message.Substring(1);

            SendMessage("<{0}> {1}", username, message);
        }

        /// <summary>
        /// Parse sent message as chat command
        /// </summary>
        /// <param name="e"></param>
        /// <returns>true if the command was successfully executed</returns>
        private static bool ProcessChatCommand(ChatMessageEventArgs e)
        {
            var commandWithoutSlash = e.Message.TrimStart('/');
            var messageArray = commandWithoutSlash
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (messageArray.Length <= 0) return false; // command not found

            var alias = messageArray[0];
            var trimmedMessageArray = new string[messageArray.Length - 1];
            if (trimmedMessageArray.Length != 0)
                Array.Copy(messageArray, 1, trimmedMessageArray, 0, messageArray.Length - 1);

            CommandManager.Instance.HandleCommand(e.Client, alias, trimmedMessageArray);

            return true;
        }

        protected internal void OnPlayerJoined(PlayerJoinedQuitEventArgs e)
        {
            if (PlayerJoined != null)
                PlayerJoined(this, e);
        }

        protected internal void OnPlayerQuit(PlayerJoinedQuitEventArgs e)
        {
            if (PlayerQuit != null)
                PlayerQuit(this, e);
        }

        public void DisconnectClient(IRemoteClient _client)
        {
            var client = (RemoteClient)_client;

            lock (ClientLock)
            {
                Clients.Remove(client);
            }

            if (client.Disconnected)
                return;

            client.Disconnected = true;

            if (client.LoggedIn)
            {
                SendMessage(ChatColor.Yellow + "{0} has left the server.", client.Username);
                GetEntityManagerForWorld(client.Dimension).DespawnEntity(client.Entity);
                GetEntityManagerForWorld(client.Dimension).FlushDespawns();
            }
            client.Save();
            client.Disconnect();
            OnPlayerQuit(new PlayerJoinedQuitEventArgs(client));

            client.Dispose();
        }

        private void AcceptClient(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                if (args.SocketError != SocketError.Success)
                    return;

                var client = new RemoteClient(this, PacketReader, PacketHandlers, args.AcceptSocket);

                lock (ClientLock)
                    Clients.Add(client);
            }
            finally
            {
                args.AcceptSocket = null;

                if (!ShuttingDown && !Listener.Server.AcceptAsync(args))
                    AcceptClient(this, args);
            }
        }

        private void TickedThreadEntry(object args)
        {
            (Action action, AutoResetEvent autoReset) = (Tuple<Action, AutoResetEvent>)args;

            while (!ShuttingDown)
            {
                autoReset.WaitOne();

                // Signal that this thread has been released and has begun processing
                // this tick.
                _cde.Signal();

                action();
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void DoEnvironment()
        {
            long start = Time.ElapsedMilliseconds;
            long limit = Time.ElapsedMilliseconds + MillisecondsPerTick;
            Profiler.Start("environment");

            Scheduler.Update();

            Profiler.Start("environment.entities");
            foreach (var manager in EntityManagers)
            {
                manager.Update();
            }
            Profiler.Done();

            if (Program.ServerConfiguration.EnableLighting)
            {
                Profiler.Start("environment.lighting");
                foreach (var lighter in WorldLighters)
                {
                    while (Time.ElapsedMilliseconds < limit && lighter.TryLightNext())
                    {
                        // This space intentionally left blank
                    }
                    //if (Time.ElapsedMilliseconds >= limit)
                    //    Log(LogCategory.Warning, "Lighting queue is backed up");
                }
                Profiler.Done();
            }

            if (Program.ServerConfiguration.EnableEventLoading)
            {
                Profiler.Start("environment.chunks");
                Tuple<IDimension, IChunk> t;
                if (ChunksToSchedule.TryTake(out t))
                    ScheduleUpdatesForChunk(t.Item1, t.Item2);
                Profiler.Done();
            }

            Profiler.Done(MillisecondsPerTick);
        }

        public bool PlayerIsWhitelisted(string client)
        {
            return AccessConfiguration.Whitelist.Contains(client, StringComparer.CurrentCultureIgnoreCase);
        }

        public bool PlayerIsBlacklisted(string client)
        {
            return AccessConfiguration.Blacklist.Contains(client, StringComparer.CurrentCultureIgnoreCase);
        }

        public bool PlayerIsOp(string client)
        {
            return AccessConfiguration.Oplist.Contains(client, StringComparer.CurrentCultureIgnoreCase);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
            }
        }

        ~MultiplayerServer()
        {
            Dispose(false);
        }
    }
}
