using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TrueCraft.Core;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.World;
using TrueCraft.Client.Input;
using TrueCraft.Client.Modules;
using TrueCraft.Client.Rendering;

using TVector3 = TrueCraft.Core.Vector3;

namespace TrueCraft.Client
{
    public class TrueCraftGame : Game
    {
        private readonly Camera _camera;
        private readonly AudioManager _audio;

        public MultiplayerClient Client { get; private set; }
        public GraphicsDeviceManager Graphics { get; private set; }
        public TextureMapper? TextureMapper { get; private set; }
        public Camera Camera { get => _camera; }
        public ConcurrentBag<Action> PendingMainThreadActions { get; set; }
        public double Bobbing { get; set; }
        public ChunkModule? ChunkModule { get; private set; }
        private ChatModule? _chatModule;
        public float ScaleFactor { get; set; }
        public GlobalVoxelCoordinates? HighlightedBlock { get; set; }
        public BlockFace HighlightedBlockFace { get; set; }
        public DateTime StartDigging { get; set; }
        public DateTime EndDigging { get; set; }
        public GlobalVoxelCoordinates? TargetBlock { get; set; }
        public AudioManager Audio { get => _audio; }
        public Texture2D? White1x1 { get; private set; }
        public PlayerControlModule? ControlModule { get; private set; }
        public SkyModule? SkyModule { get; private set; }

        private readonly List<IGameplayModule> _inputModules = new();
        private readonly List<IGameplayModule> _graphicalModules = new();
        private SpriteBatch? _spriteBatch;
        private KeyboardHandler _keyboardComponent;
        private MouseHandler _mMouseComponent;
        private GamePadHandler _gamePadComponent;
        private RenderTarget2D? _renderTarget;
        private int _threadID;

        private FontRenderer? _pixel;
        private IPEndPoint _endPoint;
        private DateTime _lastPhysicsUpdate;
        private DateTime _nextPhysicsUpdate;
        private GameTime? _gameTime;
        private DebugInfoModule? _debugInfoModule;

        public static readonly int Reach = 3;

        public IBlockRepository BlockRepository
        {
            get
            {
                return Client.Dimension.BlockRepository;
            }
        }

        [Obsolete("Inject instead")]
        public IItemRepository ItemRepository { get; set; }

        public TrueCraftGame(MultiplayerClient client, IPEndPoint endPoint)
        {
            Window.Title = "TrueCraft";
            Content.RootDirectory = "Content";
            Graphics = new GraphicsDeviceManager(this);
            Graphics.SynchronizeWithVerticalRetrace = false;
            Graphics.IsFullScreen = UserSettings.Local.IsFullscreen;
            Graphics.PreferredBackBufferWidth = UserSettings.Local.WindowResolution.Width;
            Graphics.PreferredBackBufferHeight = UserSettings.Local.WindowResolution.Height;
            Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Graphics.ApplyChanges();
            Window.ClientSizeChanged += Window_ClientSizeChanged;
            Client = client;
            _endPoint = endPoint;
            _lastPhysicsUpdate = DateTime.MinValue;
            _nextPhysicsUpdate = DateTime.MinValue;
            PendingMainThreadActions = new ConcurrentBag<Action>();
            Bobbing = 0;

            _keyboardComponent = new KeyboardHandler(this);
            Components.Add(_keyboardComponent);

            _mMouseComponent = new MouseHandler(this);
            Components.Add(_mMouseComponent);

            _gamePadComponent = new GamePadHandler(this);
            Components.Add(_gamePadComponent);

            _camera = new Camera(GraphicsDevice.Viewport.AspectRatio, 70.0f, 0.1f, 1000.0f);
            _audio = new AudioManager();
        }

        void Window_ClientSizeChanged(object? sender, EventArgs e)
        {
            if (GraphicsDevice.Viewport.Width < 640 || GraphicsDevice.Viewport.Height < 480)
                ScaleFactor = 0.5f;
            else if (GraphicsDevice.Viewport.Width < 978 || GraphicsDevice.Viewport.Height < 720)
                ScaleFactor = 1.0f;
            else
                ScaleFactor = 1.5f;
            IconRenderer.PrepareEffects(this);
            UpdateCamera();
            CreateRenderTarget();
        }

        protected override void Initialize()
        {
            base.Initialize(); // (calls LoadContent)

            UpdateCamera();

            White1x1 = new Texture2D(GraphicsDevice, 1, 1);
            White1x1.SetData<Color>(new[] { Color.White });

            _audio.LoadDefaultPacks(Content);

            SkyModule = new SkyModule(this);
            ChunkModule = new ChunkModule(this);
            _debugInfoModule = new DebugInfoModule(this, _pixel!);  // LoadContent previously called.
            _chatModule = new ChatModule(this, _pixel!);
            var hud = new HUDModule(this, _pixel!);
            var windowModule = new WindowModule(this, _pixel!);

            _graphicalModules.Add(SkyModule);
            _graphicalModules.Add(ChunkModule);
            _graphicalModules.Add(new HighlightModule(this));
            _graphicalModules.Add(hud);
            _graphicalModules.Add(_chatModule);
            _graphicalModules.Add(windowModule);
            _graphicalModules.Add(_debugInfoModule);

            _inputModules.Add(windowModule);
            _inputModules.Add(_debugInfoModule);
            _inputModules.Add(_chatModule);
            _inputModules.Add(new HUDModule(this, _pixel));
            _inputModules.Add(ControlModule = new PlayerControlModule(this));

            Client.PropertyChanged += HandleClientPropertyChanged;
            Client.Connect(_endPoint);

            ItemRepository = TrueCraft.Core.Logic.ItemRepository.Get();

            IconRenderer.CreateBlocks(this, BlockRepository);

            var centerX = GraphicsDevice.Viewport.Width / 2;
            var centerY = GraphicsDevice.Viewport.Height / 2;
            Mouse.SetPosition(centerX, centerY);

            _mMouseComponent.Scroll += OnMouseComponentScroll;
            _mMouseComponent.Move += OnMouseComponentMove;
            _mMouseComponent.ButtonDown += OnMouseComponentButtonDown;
            _mMouseComponent.ButtonUp += OnMouseComponentButtonUp;
            _keyboardComponent.KeyDown += OnKeyboardKeyDown;
            _keyboardComponent.KeyUp += OnKeyboardKeyUp;
            _gamePadComponent.ButtonDown += OnGamePadButtonDown;
            _gamePadComponent.ButtonUp += OnGamePadButtonUp;

            CreateRenderTarget();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _threadID = Thread.CurrentThread.ManagedThreadId;
        }

        public void Invoke(Action action)
        {
            if (_threadID == Thread.CurrentThread.ManagedThreadId)
                action();
            else
                PendingMainThreadActions.Add(action);
        }

        private void CreateRenderTarget()
        {
            _renderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height,
                false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
        }

        void HandleClientPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Position":
                    UpdateCamera();
                    break;
            }
        }

        protected override void LoadContent()
        {
            // Ensure we have default textures loaded.
            TextureMapper.LoadDefaults(GraphicsDevice);

            // Load any custom textures if needed.
            TextureMapper = new TextureMapper(GraphicsDevice);
            if (UserSettings.Local.SelectedTexturePack != TexturePack.Default.Name)
                TextureMapper.AddTexturePack(TexturePack.FromArchive(Path.Combine(Paths.TexturePacks,
                    UserSettings.Local.SelectedTexturePack)));

            _pixel = new FontRenderer(
                new Font(GraphicsDevice, Content.RootDirectory, "Fonts/Pixel"),
                new Font(GraphicsDevice, Content.RootDirectory, "Fonts/Pixel", FontStyle.Bold), null, null,
                new Font(GraphicsDevice, Content.RootDirectory, "Fonts/Pixel", FontStyle.Italic));

            base.LoadContent();
        }

        private void OnKeyboardKeyDown(object? sender, KeyboardKeyEventArgs e)
        {
            if (_gameTime is null)
                return;

            foreach (IGameplayModule module in _inputModules)
            {
                IInputModule? input = module as IInputModule;
                if (input?.KeyDown(_gameTime, e) ?? false)
                    break;
            }
        }

        private void OnKeyboardKeyUp(object? sender, KeyboardKeyEventArgs e)
        {
            if (_gameTime is null)
                return;

            foreach (IGameplayModule module in _inputModules)
            {
                IInputModule? input = module as IInputModule;
                if (input?.KeyUp(_gameTime, e) ?? false)
                        break;
            }
        }

        private void OnGamePadButtonUp(object? sender, GamePadButtonEventArgs e)
        {
            if (_gameTime is null)
                return;

            foreach (var module in _inputModules)
            {
                IInputModule? input = module as IInputModule;
                if (input?.GamePadButtonUp(_gameTime, e) ?? false)
                        break;
            }
        }

        private void OnGamePadButtonDown(object? sender, GamePadButtonEventArgs e)
        {
            if (_gameTime is null)
                return;

            foreach (var module in _inputModules)
            {
                IInputModule? input = module as IInputModule;
                if (input?.GamePadButtonDown(_gameTime, e) ?? false)
                        break;
            }
        }

        private void OnMouseComponentScroll(object? sender, MouseScrollEventArgs e)
        {
            if (_gameTime is null)
                return;

            foreach (var module in _inputModules)
            {
                IInputModule? input = module as IInputModule;
                if (input?.MouseScroll(_gameTime, e) ?? false)
                        break;
            }
        }

        private void OnMouseComponentButtonDown(object? sender, MouseButtonEventArgs e)
        {
            if (_gameTime is null)
                return;

            foreach (var module in _inputModules)
            {
                IInputModule? input = module as IInputModule;
                if (input?.MouseButtonDown(_gameTime, e) ?? false)
                        break;
            }
        }

        private void OnMouseComponentButtonUp(object? sender, MouseButtonEventArgs e)
        {
            if (_gameTime is null)
                return;

            foreach (var module in _inputModules)
            {
                IInputModule? input = module as IInputModule;
                if (input?.MouseButtonUp(_gameTime, e) ?? false)
                        break;
            }
        }

        private void OnMouseComponentMove(object? sender, MouseMoveEventArgs e)
        {
            if (_gameTime is null)
                return;

            foreach (var module in _inputModules)
            {
                IInputModule? input = module as IInputModule;
                if (input?.MouseMove(_gameTime, e) ?? false)
                        break;
            }
        }

        public void TakeScreenshot()
        {
            string path = Path.Combine(Paths.Screenshots, DateTime.Now.ToString("yyyy-MM-dd_H.mm.ss") + ".png");
            if (!Directory.Exists(Paths.Screenshots))
                Directory.CreateDirectory(Paths.Screenshots);
            using (var stream = File.OpenWrite(path))
                _renderTarget?.SaveAsPng(stream, _renderTarget.Width, _renderTarget.Height);
            _chatModule?.AddMessage("Screenshot saved to " + Path.GetFileName(path));
        }

        public void FlushMainThreadActions()
        {
            Action? action;
            while (PendingMainThreadActions.TryTake(out action))
                action();
        }

        protected override void Update(GameTime gameTime)
        {
            _gameTime = gameTime;

            Action? action;
            if (PendingMainThreadActions.TryTake(out action))
                action();

            IChunk? chunk;
            LocalVoxelCoordinates adjusted = Client.Dimension.FindBlockPosition(
                new GlobalVoxelCoordinates((int)Client.Position.X, 0, (int)Client.Position.Z), out chunk);
            if (chunk is not null && Client.LoggedIn)
            {
                if (chunk.GetHeight((byte)adjusted.X, (byte)adjusted.Z) != 0)
                    Client.Physics.Update(gameTime.ElapsedGameTime);
            }
            if (_nextPhysicsUpdate < DateTime.UtcNow && Client.LoggedIn)
            {
                // NOTE: This is to make the vanilla server send us chunk packets
                // We should eventually make some means of detecing that we're on a vanilla server to enable this
                // It's a waste of bandwidth to do it on a TrueCraft server
                Client.QueuePacket(new PlayerGroundedPacket { OnGround = true });
                _nextPhysicsUpdate = DateTime.UtcNow.AddMilliseconds(50);
            }

            foreach (var module in _inputModules)
                module.Update(gameTime);
            foreach (var module in _graphicalModules)
                module.Update(gameTime);

            UpdateCamera();

            base.Update(gameTime);
        }

        private void UpdateCamera()
        {
            const double bobbingMultiplier = 0.05;

            var bobbing = Bobbing * 1.5;
            var xbob = Math.Cos(bobbing + Math.PI / 2) * bobbingMultiplier;
            var ybob = Math.Sin(Math.PI / 2 - (2 * bobbing)) * bobbingMultiplier;

            _camera.Position = new TVector3(
                Client.Position.X + xbob, Client.Position.Y + Client.Size.Height + ybob, Client.Position.Z);

            _camera.Pitch = Client.Pitch;
            _camera.Yaw = Client.Yaw;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;

            Mesh.ResetStats();
            foreach (var module in _graphicalModules)
            {
                var drawable = module as IGraphicalModule;
                if (drawable != null)
                    drawable.Draw(gameTime);
            }

            GraphicsDevice.SetRenderTarget(null);

            _spriteBatch?.Begin();
            _spriteBatch?.Draw(_renderTarget, Vector2.Zero, Color.White);
            _spriteBatch?.End();

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _keyboardComponent.Dispose();
                _mMouseComponent.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
