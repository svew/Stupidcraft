using System;
using TrueCraft.API.Networking;
using System.Net.Sockets;
using TrueCraft.Core.Networking;
using System.Collections.Concurrent;
using TrueCraft.API.Server;
using TrueCraft.API.World;
using TrueCraft.API.Entities;
using TrueCraft.API;
using System.Collections.Generic;
using System.Linq;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.World;
using Ionic.Zlib;
using TrueCraft.API.Windows;
using TrueCraft.Core.Windows;
using System.Threading.Tasks;
using System.Threading;
using TrueCraft.Core.Entities;
using System.IO;
using fNbt;
using TrueCraft.API.Logging;
using TrueCraft.API.Logic;
using TrueCraft.Exceptions;
using TrueCraft.Profiling;

namespace TrueCraft
{
    public class RemoteClient : IRemoteClient, IEventSubject, IDisposable
    {
        public RemoteClient(IMultiplayerServer server, IPacketReader packetReader, PacketHandler[] packetHandlers, Socket connection)
        {
            _loadedChunks = new HashSet<GlobalChunkCoordinates>();
            Server = server;
            Inventory = new InventoryWindow(server.CraftingRepository);
            InventoryWindow.WindowChange += HandleWindowChange;
            SelectedSlot = InventoryWindow.HotbarIndex;
            CurrentWindow = InventoryWindow;
            ItemStaging = ItemStack.EmptyStack;
            KnownEntities = new List<IEntity>();
            Disconnected = false;
            EnableLogging = server.EnableClientLogging;
            NextWindowID = 1;
            _connection = connection;
            _socketPool = new SocketAsyncEventArgsPool(100, 200, 65536);
            PacketReader = packetReader;
            PacketHandlers = packetHandlers;

            StartReceive();
        }

        public event EventHandler Disposed;

        /// <summary>
        /// A list of entities that this client is aware of.
        /// </summary>
        internal List<IEntity> KnownEntities { get; set; }
        internal sbyte NextWindowID { get; set; }

        //public NetworkStream NetworkStream { get; set; }
        public IMinecraftStream MinecraftStream { get; internal set; }
        public string Username { get; internal set; }
        public bool LoggedIn { get; internal set; }
        public IMultiplayerServer Server { get; }
        public IWorld World { get; internal set; }
        public IWindow Inventory { get; }
        public short SelectedSlot { get; internal set; }
        public ItemStack ItemStaging { get; set; }
        public IWindow CurrentWindow { get; internal set; }
        public bool EnableLogging { get; set; }
        public DateTime ExpectedDigComplete { get; set; }

        // Set to true when this object has been disposed.
        private bool _thisDisposed = false;

        private Socket _connection;

        private SocketAsyncEventArgsPool _socketPool;

        public IPacketReader PacketReader { get; private set; }

        private PacketHandler[] PacketHandlers { get; set; }

        private IEntity _Entity;

        private long disconnected;

        public bool Disconnected
        {
            get
            {
                return Interlocked.Read(ref disconnected) == 1;
            }
            internal set
            {
                Interlocked.CompareExchange(ref disconnected, value ? 1 : 0, value ? 0 : 1);
            }
        }

        public IEntity Entity
        {
            get
            {
                return _Entity;
            }
            internal set
            {
                var player = _Entity as PlayerEntity;
                if (player != null)
                    player.PickUpItem -= HandlePickUpItem;
                _Entity = value;
                player = _Entity as PlayerEntity;
                if (player != null)
                    player.PickUpItem += HandlePickUpItem;
            }
        }

        void HandlePickUpItem(object sender, EntityEventArgs e)
        {
            var packet = new CollectItemPacket(e.Entity.EntityID, Entity.EntityID);
            QueuePacket(packet);
            var manager = Server.GetEntityManagerForWorld(World);
            foreach (var client in manager.ClientsForEntity(Entity))
                client.QueuePacket(packet);
            Inventory.PickUpStack((e.Entity as ItemEntity).Item);
        }

        public ItemStack SelectedItem
        {
            get
            {
                return Inventory[SelectedSlot];
            }
        }

        public InventoryWindow InventoryWindow
        {
            get
            {
                return Inventory as InventoryWindow;
            }
        }

        internal int ChunkRadius { get; set; }

        private readonly object _loadedChunkLock = new object();
        private HashSet<GlobalChunkCoordinates> _loadedChunks;

        public bool DataAvailable
        {
            get
            {
                return true;
            }
        }

        public bool Load()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "players", Username + ".nbt");
            if (Program.ServerConfiguration.Singleplayer)
                path = Path.Combine(((World)World).BaseDirectory, "player.nbt");
            if (!File.Exists(path))
                return false;
            try
            {
                var nbt = new NbtFile(path);
                Entity.Position = new Vector3(
                    nbt.RootTag["position"][0].DoubleValue,
                    nbt.RootTag["position"][1].DoubleValue,
                    nbt.RootTag["position"][2].DoubleValue);
                Inventory.SetSlots(((NbtList)nbt.RootTag["inventory"]).Select(t => ItemStack.FromNbt(t as NbtCompound)).ToArray());
                (Entity as PlayerEntity).Health = nbt.RootTag["health"].ShortValue;
                Entity.Yaw = nbt.RootTag["yaw"].FloatValue;
                Entity.Pitch = nbt.RootTag["pitch"].FloatValue;
            }
            catch { /* Who cares */ }
            return true;
        }

        public void Save()
        {
            // The remote client may be disconnected prior to setting the World property.
            if (object.ReferenceEquals(World, null))
                return;

            var path = Path.Combine(Directory.GetCurrentDirectory(), "players", Username + ".nbt");
            if (Program.ServerConfiguration.Singleplayer)
                path = Path.Combine(((World)World).BaseDirectory, "player.nbt");
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (Entity == null) // I didn't think this could happen but null reference exceptions have been repoted here
                return;
            var nbt = new NbtFile(new NbtCompound("player", new NbtTag[]
                {
                    new NbtString("username", Username),
                    new NbtList("position", new[]
                    {
                        new NbtDouble(Entity.Position.X),
                        new NbtDouble(Entity.Position.Y),
                        new NbtDouble(Entity.Position.Z)
                    }),
                    // TODO BUG: this saves the items in the Crafting area as part of the player's inventory.
                    //           They should be dropped when the player closes the window.
                    new NbtList("inventory", Inventory.GetSlots().Select(s => s.ToNbt())),
                    new NbtShort("health", (Entity as PlayerEntity).Health),
                    new NbtFloat("yaw", Entity.Yaw),
                    new NbtFloat("pitch", Entity.Pitch),
                }
            ));
            nbt.SaveToFile(path, NbtCompression.ZLib);
        }

        public void OpenWindow(IWindow window)
        {
            CurrentWindow = window;
            window.Client = this;
            window.ID = NextWindowID++;
            if (NextWindowID < 0) NextWindowID = 1;
            QueuePacket(new OpenWindowPacket(window.ID, window.Type, window.Name, (sbyte)window.Length2));
            QueuePacket(new WindowItemsPacket(window.ID, window.GetSlots()));
            window.WindowChange += HandleWindowChange;
        }

        public void CloseWindow(bool clientInitiated = false)
        {
            if (!clientInitiated)
                QueuePacket(new CloseWindowPacket(CurrentWindow.ID));
            CurrentWindow.CopyToInventory(Inventory);
            CurrentWindow.Dispose();
            CurrentWindow = InventoryWindow;
        }

        public void Log(string message, params object[] parameters)
        {
            if (EnableLogging)
                SendMessage(ChatColor.Gray + string.Format("[" + DateTime.UtcNow.ToShortTimeString() + "] " + message, parameters));
        }

        public void QueuePacket(IPacket packet)
        {
            if (Disconnected || (_connection != null && !_connection.Connected))
                return;

            using (MemoryStream writeStream = new MemoryStream())
            {
                using (MinecraftStream ms = new MinecraftStream(writeStream))
                {
                    writeStream.WriteByte(packet.ID);
                    packet.WritePacket(ms);
                }

                byte[] buffer = writeStream.ToArray();

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.UserToken = packet;
                args.Completed += OperationCompleted;
                args.SetBuffer(buffer, 0, buffer.Length);

                if (_connection != null)
                {
                    if (!_connection.SendAsync(args))
                        OperationCompleted(this, args);
                }
            }
        }

        private void StartReceive()
        {
            // Check if this RemoteClient has been disposed.
            if (object.ReferenceEquals(_socketPool, null))
                return;

            SocketAsyncEventArgs args = _socketPool.Get();
            args.Completed += OperationCompleted;

            if (!_connection.ReceiveAsync(args))
                OperationCompleted(this, args);
        }

        private void OperationCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (_thisDisposed) return;

            e.Completed -= OperationCompleted;

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessNetwork(e);

                    _socketPool?.Add(e);
                    break;
                case SocketAsyncOperation.Send:
                    IPacket packet = e.UserToken as IPacket;

                    if (packet is DisconnectPacket)
                        Server.DisconnectClient(this);

                    e.SetBuffer(null, 0, 0);
                    break;
            }

            if (_connection != null)
                if (!_connection.Connected && !Disconnected)
                    Server.DisconnectClient(this);
        }

        private void ProcessNetwork(SocketAsyncEventArgs e)
        {
            if (_connection == null || !_connection.Connected)
                return;

            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                Server.DisconnectClient(this);
                return;
            }

            var packets = PacketReader.ReadPackets(this, e.Buffer, e.Offset, e.BytesTransferred);
            try
            {
                foreach (IPacket packet in packets)
                {
                    if (PacketHandlers[packet.ID] != null)
                    {
                        try
                        {
                            PacketHandlers[packet.ID](packet, this, Server);
                        }
                        catch (PlayerDisconnectException)
                        {
                            Server.DisconnectClient(this);
                        }
                        catch (Exception ex)
                        {
                            Server.Log(LogCategory.Notice, "Disconnecting client due to exception in network worker");
                            Server.Log(LogCategory.Debug, ex.ToString());
                            Server.Log(LogCategory.Debug, ex.StackTrace.ToString());

                            Server.DisconnectClient(this);
                        }
                    }
                    else
                    {
                        Log("Unhandled packet {0}", packet.GetType().Name);
                    }
                }
            }
            catch (NotSupportedException)
            {
                Server.Log(LogCategory.Debug, "Disconnecting client due to unsupported packet received.");
                return;
            }

            StartReceive();
        }

        public void Disconnect()
        {
            if (Disconnected)
                return;

            Disconnected = true;

            _connection.Shutdown(SocketShutdown.Both);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += DisconnectCompleted;
            if (!_connection.DisconnectAsync(args))
            {
                DisconnectCompleted(_connection, args);
                args.Dispose();
            }
        }

        private void DisconnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            _connection.Close();
        }

        public void SendMessage(string message)
        {
            var parts = message.Split('\n');
            foreach (var part in parts)
                QueuePacket(new ChatMessagePacket(part));
        }

        internal void ExpandChunkRadius(IMultiplayerServer server)
        {
            if (this.Disconnected)
                return;
            Task.Factory.StartNew(() =>
            {
                if (ChunkRadius < 8) // TODO: Allow customization of this number
                {
                    ChunkRadius++;
                    server.Scheduler.ScheduleEvent("client.update-chunks", this,
                        TimeSpan.Zero, s => UpdateChunks());
                    server.Scheduler.ScheduleEvent("remote.chunks", this,
                        TimeSpan.FromSeconds(1), ExpandChunkRadius);
                }
            });
        }

        internal void SendKeepAlive(IMultiplayerServer server)
        {
            QueuePacket(new KeepAlivePacket());
            server.Scheduler.ScheduleEvent("remote.keepalive", this, TimeSpan.FromSeconds(10), SendKeepAlive);
        }

        internal void UpdateChunks(bool block = false)
        {
            var toLoad = new List<Tuple<GlobalChunkCoordinates, IChunk>>();
            int cr = ChunkRadius;
            GlobalChunkCoordinates entityChunk = (GlobalChunkCoordinates)Entity.Position;

            Profiler.Start("client.new-chunks");
            lock(_loadedChunkLock)
                for (int x = -cr; x < cr; x++)
                {
                    for (int z = -cr; z < cr; z++)
                    {
                        GlobalChunkCoordinates coords = new GlobalChunkCoordinates(entityChunk.X + x, entityChunk.Z + z);
                        if (!_loadedChunks.Contains(coords))
                            toLoad.Add(new Tuple<GlobalChunkCoordinates, IChunk>(
                                coords, World.GetChunk(coords, generate: block)));
                    }
                }
            Profiler.Done();

            var encode = new Action(() =>
            {
                Profiler.Start("client.encode-chunks");
                foreach (var tup in toLoad)
                {
                    var coords = tup.Item1;
                    var chunk = tup.Item2;
                    if (chunk == null)
                        chunk = World.GetChunk(coords);
                    chunk.LastAccessed = DateTime.UtcNow;
                    LoadChunk(chunk);
                }
                Profiler.Done();
            });
            if (block)
                encode();
            else
                Task.Factory.StartNew(encode);

            Profiler.Start("client.old-chunks");
            List<GlobalChunkCoordinates> unload = new List<GlobalChunkCoordinates>(2 * cr + 1);
            lock(_loadedChunkLock)
                unload.AddRange(_loadedChunks.Where((a) => Math.Abs(a.X - entityChunk.X) > cr || Math.Abs(a.Z - entityChunk.Z) > cr));
            unload.ForEach((a) => UnloadChunk(a));
            Profiler.Done();

            Profiler.Start("client.update-entities");
            ((EntityManager)Server.GetEntityManagerForWorld(World)).UpdateClientEntities(this);
            Profiler.Done();
        }

        internal void UnloadAllChunks()
        {
            lock(_loadedChunkLock)
                while (_loadedChunks.Any())
                    UnloadChunk(_loadedChunks.First());
        }

        internal void LoadChunk(IChunk chunk)
        {
            QueuePacket(new ChunkPreamblePacket(chunk.Coordinates.X, chunk.Coordinates.Z));
            QueuePacket(CreatePacket(chunk));
            lock(_loadedChunkLock)
                _loadedChunks.Add(chunk.Coordinates);

            // TODO:
            //Server.Scheduler.ScheduleEvent("client.finalize-chunks", this,
            //    TimeSpan.Zero, server =>
            //    {
            //        return;
            //        foreach (var kvp in chunk.TileEntities)
            //        {
            //            var coords = kvp.Key;
            //            var descriptor = new BlockDescriptor
            //            {
            //                Coordinates = coords + new Coordinates3D(chunk.X, 0, chunk.Z),
            //                Metadata = chunk.GetMetadata(coords),
            //                ID = chunk.GetBlockID(coords),
            //                BlockLight = chunk.GetBlockLight(coords),
            //                SkyLight = chunk.GetSkyLight(coords)
            //            };
            //            var provider = Server.BlockRepository.GetBlockProvider(descriptor.ID);
            //            provider.TileEntityLoadedForClient(descriptor, World, kvp.Value, this);
            //        }
            //    });
        }

        internal void UnloadChunk(GlobalChunkCoordinates position)
        {
            QueuePacket(new ChunkPreamblePacket(position.X, position.Z, false));
            lock(_loadedChunkLock)
                _loadedChunks.Remove(position);
        }

        void HandleWindowChange(object sender, WindowChangeEventArgs e)
        {
            if (!(sender is InventoryWindow))
            {
                QueuePacket(new SetSlotPacket((sender as IWindow).ID, (short)e.SlotIndex, e.Value.ID, e.Value.Count, e.Value.Metadata));
                return;
            }

            QueuePacket(new SetSlotPacket(0, (short)e.SlotIndex, e.Value.ID, e.Value.Count, e.Value.Metadata));

            if (e.SlotIndex == SelectedSlot)
            {
                var notified = Server.GetEntityManagerForWorld(World).ClientsForEntity(Entity);
                foreach (var c in notified)
                    c.QueuePacket(new EntityEquipmentPacket(Entity.EntityID, 0, SelectedItem.ID, SelectedItem.Metadata));
            }
            if (e.SlotIndex >= InventoryWindow.ArmorIndex && e.SlotIndex < InventoryWindow.ArmorIndex + InventoryWindow.Armor.Length)
            {
                short slot = (short)(4 - (e.SlotIndex - InventoryWindow.ArmorIndex));
                var notified = Server.GetEntityManagerForWorld(World).ClientsForEntity(Entity);
                foreach (var c in notified)
                    c.QueuePacket(new EntityEquipmentPacket(Entity.EntityID, slot, e.Value.ID, e.Value.Metadata));
            }
        }

        private static ChunkDataPacket CreatePacket(IChunk chunk)
        {
            var X = chunk.Coordinates.X;
            var Z = chunk.Coordinates.Z;

            Profiler.Start("client.encode-chunks.compress");
            byte[] result;
            using (var ms = new MemoryStream())
            {
                using (var msOut = new MemoryStream(chunk.Data))
                using (var deflate = new ZlibStream(msOut, CompressionMode.Compress, CompressionLevel.BestSpeed))
                    deflate.CopyTo(ms);
                result = ms.ToArray();
            }
            Profiler.Done();

            return new ChunkDataPacket(X * Chunk.Width, 0, Z * Chunk.Depth,
                Chunk.Width, Chunk.Height, Chunk.Depth, result);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _thisDisposed = true;

            if (disposing)
            {
                IPacketSegmentProcessor processor;
                while (!PacketReader.Processors.TryRemove(this, out processor))
                    Thread.Sleep(1);

                Disconnect();

                _socketPool?.Dispose();
                _socketPool = null;
                _connection?.Dispose();
                _connection = null;

                if (Disposed != null)
                    Disposed(this, null);
            }

        }
    }
}
