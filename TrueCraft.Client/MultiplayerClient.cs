using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Client.Events;
using TrueCraft.Core.Logic;
using System.ComponentModel;
using System.IO;
using TrueCraft.Core;
using TrueCraft.Core.Physics;
using TrueCraft.Core.World;
using TrueCraft.Core.Inventory;
using TrueCraft.Client.Inventory;
using TrueCraft.Client.World;
using TrueCraft.Core.Entities;
using System.Collections.Generic;

namespace TrueCraft.Client
{
    public delegate void PacketHandler(IPacket packet, MultiplayerClient client);

    // TODO: Single Responsibility Principle - This should be "Has a PlayerEntity"
    //       NOT "Is an IEntity"
    public class MultiplayerClient : IEntity, INotifyPropertyChanged, IDisposable // TODO: Make IMultiplayerClient and so on
    {
        public event EventHandler<ChatMessageEventArgs>? ChatMessage;
        public event EventHandler<ChunkEventArgs>? ChunkModified;
        public event EventHandler<ChunkEventArgs>? ChunkLoaded;
        public event EventHandler<ChunkEventArgs>? ChunkUnloaded;
        public event EventHandler<BlockChangeEventArgs>? BlockChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        private long connected;
        private int hotbarSelection;
        private int _entityID = -1;

        public TrueCraftUser User { get; }

        public IDimension Dimension { get; set; }

        private IPhysicsEngine _physics;
        public IPhysicsEngine Physics { get => _physics; }

        public bool LoggedIn { get; internal set; }
        public int EntityID { get => _entityID; internal set => _entityID = value; }

        public IInventoryWindow<ISlot> InventoryWindow { get; }
        public ISlots<ISlot> Inventory { get; private set; }
        public ISlots<ISlot> Hotbar { get; private set; }
        public ISlots<ISlot> Armor { get => InventoryWindow.Armor; }
        public ISlots<ISlot> CraftingGrid { get => InventoryWindow.CraftingGrid; }

        public int Health { get; set; }

        public IWindow<ISlot>? CurrentWindow { get; set; }

        public bool Connected
        {
            get
            {
                return Interlocked.Read(ref connected) == 1;
            }
        }

        public int HotbarSelection
        {
            get { return hotbarSelection; }
            set
            {
                hotbarSelection = value;
                QueuePacket(new ChangeHeldItemPacket((short)value));
            }
        }

        private TcpClient _client;

        private PacketReader _packetReader;

        private readonly PacketHandler[] PacketHandlers;

        private SocketAsyncEventArgsPool _socketPool;

        public MultiplayerClient(IServiceLocator serviceLocator, TrueCraftUser user)
        {
            User = user;
            _client = new TcpClient();
            _packetReader = new PacketReader();
            _packetReader.RegisterCorePackets();
            PacketHandlers = new PacketHandler[0x100];
            Handlers.PacketHandlers.RegisterHandlers(this);

            Dimension = new Dimension(serviceLocator.BlockRepository, serviceLocator.ItemRepository);
            _physics = new PhysicsEngine(Dimension);

            _socketPool = new SocketAsyncEventArgsPool(100, 200, 65536);
            connected = 0;
            Health = 20;

            ISlotFactory<ISlot> slotFactory = new SlotFactory<ISlot>();
            IItemRepository itemRepository = serviceLocator.ItemRepository;
            Inventory = new Slots<ISlot>(itemRepository, slotFactory.GetSlots(itemRepository, 27), 9);   // TODO hard-coded constants
            Hotbar = new Slots<ISlot>(itemRepository, slotFactory.GetSlots(itemRepository,9), 9);        // TODO hard-coded constants

            IInventoryFactory<ISlot> factory = new InventoryFactory<ISlot>();
            InventoryWindow = (InventoryWindow)factory.NewInventoryWindow(itemRepository,
                serviceLocator.CraftingRepository, slotFactory, Inventory, Hotbar);
        }

        public void RegisterPacketHandler(byte packetId, PacketHandler handler)
        {
            PacketHandlers[packetId] = handler;
        }

        public void Connect(IPEndPoint endPoint)
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += Connection_Completed;
            args.RemoteEndPoint = endPoint;

            if (!_client.Client.ConnectAsync(args))
                Connection_Completed(this, args);
        }

        private void Connection_Completed(object? sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    Interlocked.CompareExchange(ref connected, 1, 0);

                    Physics.AddEntity(this);

                    StartReceive();
                    QueuePacket(new HandshakePacket(User.Username));
                }
                else
                {
                    throw new Exception("Could not connect to server!");
                }
            }
            finally
            {
                e.Dispose();
            }
        }

        public void Disconnect()
        {
            if (!Connected)
                return;

            QueuePacket(new DisconnectPacket("Disconnecting"));
            
            Interlocked.CompareExchange(ref connected, 0, 1);
        }

        public void SendMessage(string message)
        {
            var parts = message.Split('\n');
            foreach (var part in parts)
                QueuePacket(new ChatMessagePacket(part));
        }

        public void QueuePacket(IPacket packet)
        {
            if (!Connected || (_client != null && !_client.Connected))
                return;

            using (MemoryStream writeStream = new MemoryStream())
            {
                using (MinecraftStream ms = new MinecraftStream(writeStream))
                {
                    ms.WriteUInt8(packet.ID);
                    packet.WritePacket(ms);
                }

                byte[] buffer = writeStream.ToArray();

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.UserToken = packet;
                args.Completed += OperationCompleted;
                args.SetBuffer(buffer, 0, buffer.Length);

                if (_client != null && !_client.Client.SendAsync(args))
                    OperationCompleted(this, args);
            }
        }

        private void StartReceive()
        {
            SocketAsyncEventArgs args = _socketPool.Get();
            args.Completed += OperationCompleted;

            if (!_client.Client.ReceiveAsync(args))
                OperationCompleted(this, args);
        }

        private void OperationCompleted(object? sender, SocketAsyncEventArgs e)
        {
            e.Completed -= OperationCompleted;

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessNetwork(e);

                    _socketPool.Add(e);
                    break;
                case SocketAsyncOperation.Send:
                    if (e.UserToken is DisconnectPacket)
                    {
                        _client.Client.Shutdown(SocketShutdown.Send);
                        _client.Close();
                    }

                    e.SetBuffer(null, 0, 0);
                    break;
            }
        }

        private void ProcessNetwork(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                Disconnect();
                return;
            }

            if (e.Buffer is not null)
            {
                IEnumerable<IPacket> packets = _packetReader.ReadPackets(this, e.Buffer, e.Offset, e.BytesTransferred, false);

                foreach (IPacket packet in packets)
                    if (PacketHandlers[packet.ID] != null)
                        PacketHandlers[packet.ID](packet, this);
            }

            StartReceive();
        }

        protected internal void OnChatMessage(ChatMessageEventArgs e)
        {
            if (ChatMessage != null) ChatMessage(this, e);
        }

        protected internal void OnChunkLoaded(ChunkEventArgs e)
        {
            if (ChunkLoaded != null) ChunkLoaded(this, e);
        }

        protected internal void OnChunkUnloaded(ChunkEventArgs e)
        {
            if (ChunkUnloaded != null) ChunkUnloaded(this, e);
        }

        protected internal void OnChunkModified(ChunkEventArgs e)
        {
            if (ChunkModified != null) ChunkModified(this, e);
        }

        protected internal void OnBlockChanged(BlockChangeEventArgs e)
        {
            if (BlockChanged != null) BlockChanged(this, e);
        }

        #region IEntity implementation

        public const double Width = 0.6;
        public const double Height = 1.62;
        public const double Depth = 0.6;

        public void TerrainCollision(Vector3 collisionPoint, Vector3 collisionDirection)
        {
            // This space intentionally left blank
        }

        public BoundingBox BoundingBox
        {
            get
            {
                var pos = Position - new Vector3(Width / 2, 0, Depth / 2);
                return new BoundingBox(pos, pos + Size);
            }
        }

        public Size Size
        {
            get { return new Size(Width, Height, Depth); }
        }

        /// <inheritdoc />
        public bool BeginUpdate()
        {
            return true;
        }

        /// <inheritdoc />
        public void EndUpdate(Vector3 newPosition, Vector3 newVelocity)
        {
            bool positionChanged = (newPosition != _position);
            bool velocityChanged = (newVelocity != _velocity);

            if (positionChanged)
                _position = newPosition;
            if (velocityChanged)
                _velocity = newVelocity;

            if (positionChanged)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
            if (velocityChanged)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Velocity)));
        }

        public float Yaw { get; set; }
        public float Pitch { get; set; }

        private Vector3 _position;
        public Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                if (_position == value)
                    return;

                _position = value;
                QueuePacket(new PlayerPositionAndLookPacket(value.X, value.Y, value.Y + Height,
                    value.Z, Yaw, Pitch, false));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
            }
        }

        public Vector3 Velocity { get; set; }

        public float AccelerationDueToGravity
        {
            get
            {
                return (float)GameConstants.AccelerationDueToGravity;
            }
        }

        public float Drag
        {
            get
            {
                return 0.40f;
            }
        }

        public float TerminalVelocity
        {
            get
            {
                return (float)GameConstants.TerminalVelocity;
            }
        }

        public IPacket SpawnPacket => throw new NotImplementedException();

        int IEntity.EntityID { get => _entityID; set => _entityID = value; }
        public bool Despawned { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime SpawnTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public MetadataDictionary Metadata => throw new NotImplementedException();

        public Core.Server.IEntityManager EntityManager => throw new NotImplementedException();

        public bool SendMetadataToClients => throw new NotImplementedException();

        public void Update(Core.Server.IEntityManager entityManager)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable implementation
        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disconnect();
                _socketPool?.Dispose();
                _socketPool = null!;
                Inventory = null!;
                Hotbar = null!;
                CurrentWindow = null;
            }
        }

        ~MultiplayerClient()
        {
            Dispose(false);
        }
        #endregion
    }
}
