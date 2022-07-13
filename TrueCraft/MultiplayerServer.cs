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
using TrueCraft.World;

namespace TrueCraft
{
    public class MultiplayerServer : IMultiplayerServer, IDisposable
    {
        private static MultiplayerServer? _singleton = null;

        private readonly IServiceLocator _serviceLocator;

        public event EventHandler<ChatMessageEventArgs>? ChatMessageReceived;
        public event EventHandler<PlayerJoinedQuitEventArgs>? PlayerJoined;
        public event EventHandler<PlayerJoinedQuitEventArgs>? PlayerQuit;

        private IWorld? _world = null;

        private Thread _masterTick;
        private List<AutoResetEvent> _lstAutoResetEvents;
        private CountdownEvent _cde;
        private Thread _environmentWorker;
        private AutoResetEvent _environmentAutoReset;



        private TcpListener? _listener;
        private readonly PacketHandler[] PacketHandlers;
        private IList<ILogProvider> LogProviders;
        private Stopwatch Time;
        private ConcurrentBag<Tuple<IDimension, IChunk>> ChunksToSchedule;
        internal object ClientLock = new object();

        private readonly QueryProtocol _queryProtocol;

        public MultiplayerServer(IServiceLocator serviceLocator)
        {
            TrueCraft.Core.WhoAmI.Answer = Core.IAm.Server;

            _serviceLocator = serviceLocator;

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

            LogProviders = new List<ILogProvider>();
            Scheduler = new EventScheduler(this);

            PendingBlockUpdates = new Queue<BlockUpdate>();
            EnableClientLogging = false;
            _queryProtocol = new TrueCraft.QueryProtocol(this);
            WorldLighters = new List<Lighting>();
            ChunksToSchedule = new ConcurrentBag<Tuple<IDimension, IChunk>>();
            Time = new Stopwatch();

            AccessConfiguration = Configuration.LoadConfiguration<AccessConfiguration>("access.yaml");

            reader.RegisterCorePackets();
            Handlers.PacketHandlers.RegisterHandlers(this);
        }

        [Obsolete("Inject instance of IMultiplayerServer instead")]
        public static MultiplayerServer Get()
        {
            if (_singleton is null)
                _singleton = new MultiplayerServer();

            return _singleton;
        }

        public IAccessConfiguration AccessConfiguration { get; internal set; }

        public IPacketReader PacketReader { get; private set; }
        public IList<IRemoteClient> Clients { get; private set; }

        // <inheritdoc />
        public object? World
        {
            get => _world;
            set
            {
                if (value is null)
                    throw new ArgumentException("World must not be set to null.");
                if (_world is not null)
                    throw new InvalidOperationException($"{nameof(World)} is already set.");

                _world = (IWorld)value;
                foreach (IDimensionServer d in _world)
                    // TODO: Once the nether is created, there will no longer be a null dimension.
                    if (d is not null)
                        d.BlockChanged += HandleBlockChanged;
            }
        }

        public IList<Lighting> WorldLighters { get; set; }
        public IEventScheduler Scheduler { get; private set; }

        [Obsolete]
        public IBlockRepository BlockRepository { get => _serviceLocator.BlockRepository; }
        [Obsolete]
        public IItemRepository ItemRepository { get => _serviceLocator.ItemRepository; }

        public bool EnableClientLogging { get; set; }

        public IPEndPoint? EndPoint { get; private set; }

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

        internal bool ShuttingDown { get; private set; }
        

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
            if (_world is null)
                throw new InvalidOperationException("Start called before World was set.");

            Scheduler.DisabledEvents.Clear();
            if (Program.ServerConfiguration?.DisabledEvents is not null)
                Program.ServerConfiguration.DisabledEvents.ToList().ForEach(
                    ev => Scheduler.DisabledEvents.Add(ev));
            ShuttingDown = false;
            Time.Reset();
            Time.Start();
            _listener = new TcpListener(endPoint);
            _listener.Start();
            EndPoint = (IPEndPoint)_listener.LocalEndpoint;

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += AcceptClient;

            if (!_listener.Server.AcceptAsync(args))
                AcceptClient(this, args);
            
            Log(LogCategory.Notice, "Running TrueCraft server on {0}", EndPoint);

            // Start all threads
            _environmentWorker.Start(new Tuple<Action, AutoResetEvent>(DoEnvironment, _environmentAutoReset));
            _masterTick.Start();

            if(Program.ServerConfiguration?.Query ?? ServerConfiguration.QueryDefault)
                _queryProtocol.Start();
        }

        public void Stop()
        {
            ShuttingDown = true;
            _listener?.Stop();
            if(Program.ServerConfiguration?.Query ?? ServerConfiguration.QueryDefault)
                _queryProtocol.Stop();

            _world?.Save();

            // NOTE: DisconnectClient modifies the Clients collection!
            for (int j = Clients.Count - 1; j >= 0; j --)
                DisconnectClient(Clients[j]);
        }

        void ScheduleUpdatesForChunk(IDimension dimension, IChunk chunk)
        {
            chunk.UpdateHeightMap();
            // NOTE: adhoc coordinate conversion.
            int xg = chunk.Coordinates.X * WorldConstants.ChunkWidth;
            int zg = chunk.Coordinates.Z * WorldConstants.ChunkDepth;
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
                        if (id == 0)  // TODO: Fix hard-coded air block ID.
                            continue;
                        coords = new GlobalVoxelCoordinates(xg + x, y, zg + z);
                        var provider = _serviceLocator.BlockRepository.GetBlockProvider(id);
                        provider.BlockLoadedFromChunk(this, dimension, coords);
                    }
                }
            }
        }

        // TODO: remove this method; it belongs to the dimension
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
                    var provider = _serviceLocator.BlockRepository.GetBlockProvider(descriptor.ID);
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
                SendChatMessage(e.Client.Username!, message);
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
                SendMessage(ChatColor.Yellow + "{0} has left the server.", client.Username!);
                IEntityManager manager = ((IDimensionServer)_client.Dimension!).EntityManager;
                manager.DespawnEntity(client.Entity!);
                manager.FlushDespawns();
            }
            client.Save();
            client.Disconnect();
            OnPlayerQuit(new PlayerJoinedQuitEventArgs(client));

            client.Dispose();
        }

        private void AcceptClient(object? sender, SocketAsyncEventArgs args)
        {
            try
            {
                if (args.SocketError != SocketError.Success)
                    return;

                var client = new RemoteClient(this, PacketReader, PacketHandlers, args.AcceptSocket!);

                lock (ClientLock)
                    Clients.Add(client);
            }
            finally
            {
                args.AcceptSocket = null;

                if (!ShuttingDown && !_listener!.Server.AcceptAsync(args))
                    AcceptClient(this, args);
            }
        }

        private void TickedThreadEntry(object? args)
        {
            if (args is null)
                throw new ArgumentNullException(nameof(args));

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
            // TODO: once the Nether is created, make this "server" not nullable.
            foreach (IDimensionServer? server in _world!)
                server?.EntityManager.Update();
            Profiler.Done();

            if (Program.ServerConfiguration?.EnableLighting ?? ServerConfiguration.EnableLightingDefault)
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

            if (Program.ServerConfiguration?.EnableEventLoading ?? ServerConfiguration.EnableEventLoadingDefault)
            {
                Profiler.Start("environment.chunks");
                Tuple<IDimension, IChunk>? t;
                if (ChunksToSchedule.TryTake(out t))
                    ScheduleUpdatesForChunk(t.Item1, t.Item2);
                Profiler.Done();
            }

            Profiler.Done(MillisecondsPerTick);
        }

        private void HandleBlockChanged(object? sender, BlockChangeEventArgs e)
        {
            IDimensionServer sendingDimension = (IDimensionServer)sender!;

            foreach (IRemoteClient client in Clients)
                // TODO: Confirm that the block is within this Client's Chunk Radius.
                // TODO: what if client just logged out?
                if (client.Dimension == sendingDimension)
                    client.QueuePacket(new BlockChangePacket(e.Position.X, (sbyte)e.Position.Y, e.Position.Z,
                            (sbyte)e.NewBlock.ID, (sbyte)e.NewBlock.Metadata));
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
