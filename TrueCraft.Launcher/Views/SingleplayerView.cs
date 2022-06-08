using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Gdk;
using Gtk;
using TrueCraft.Core;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;
using TrueCraft.Launcher.Singleplayer;
using TrueCraft.World;

namespace TrueCraft.Launcher.Views
{
    public class SingleplayerView : VBox
    {
        private LauncherWindow _window;
        private Label _singleplayerLabel;

        private TreeView _worldListView;
        private ListStore _worldListStore;

        private Button _createWorldButton;
        private Button _deleteWorldButton;
        private Button _playButton;
        private Button _backButton;
        private VBox _createWorldBox;
        private Entry _newWorldName;
        private Entry _newWorldSeed;
        private Button _newWorldCommit;
        private Button _newWorldCancel;
        private Label _progressLabel;
        private ProgressBar _progressBar;

        private readonly Worlds _worlds;

        public SingleplayerView(LauncherWindow window)
        {
            _worlds = new Worlds(Paths.Worlds);

            _window = window;
            this.SetSizeRequest(250, -1);

            _singleplayerLabel = new Label("Singleplayer")
            {
                Justify = Justification.Center
            };

            _worldListStore = new ListStore(typeof(string), typeof(WorldInfo));
            _worldListView = new TreeView(_worldListStore);
            _worldListView.SetSizeRequest(-1, 200);
            _worldListView.HeadersVisible = false;
            AddWorldColumns(_worldListView);
            TreeSelection worldListSelection = _worldListView.Selection;
            worldListSelection.Mode = SelectionMode.Single;
            worldListSelection.Changed += (sender, e) =>
            {
               int selectedCount = worldListSelection.CountSelectedRows();
               _playButton.Sensitive = (selectedCount == 1);
               _deleteWorldButton.Sensitive = (selectedCount == 1);
            };

            _createWorldButton = new Button("New world");
            _deleteWorldButton = new Button("Delete") { Sensitive = false };
            _playButton = new Button("Play") { Sensitive = false };
            _backButton = new Button("Back");
            _createWorldBox = new VBox() { Visible = false };
            _newWorldName = new Entry() { PlaceholderText = "Name" };
            _newWorldSeed = new Entry() { PlaceholderText = "Seed (optional)" };
            _newWorldCommit = new Button("Create") { Sensitive = false };
            _newWorldCancel = new Button("Cancel");

            _progressLabel = new Label("Loading world...") { Visible = false };
            // TODO: we have to call Pulse on the Progress Bar once in a while.
            _progressBar = new ProgressBar() { Visible = false, Fraction = 0 };

            _backButton.Clicked += (sender, e) => _window.ShowMainMenuView();
            _createWorldButton.Clicked += (sender, e) =>
            {
                _createWorldBox.Visible = true;
            };
            _newWorldCancel.Clicked += (sender, e) =>
            {
                _createWorldBox.Visible = false;
            };
            _newWorldName.Changed += (sender, e) =>
            {
                _newWorldCommit.Sensitive = !string.IsNullOrEmpty(_newWorldName.Text);
            };
            _newWorldCommit.Clicked += NewWorldCommit_Clicked;

            _playButton.Clicked += PlayButton_Clicked;
            _deleteWorldButton.Clicked += DeleteButton_Clicked;

            foreach (WorldInfo worldInfo in _worlds)
            {
                TreeIter row = _worldListStore.Append();
                _worldListStore.SetValue(row, 0, worldInfo.Name);
                _worldListStore.SetValue(row, 1, worldInfo);
            }

            var createDeleteHbox = new HBox();
            createDeleteHbox.PackStart(_createWorldButton, true, false, 0);
            createDeleteHbox.PackStart(_deleteWorldButton, true, false, 0);

            _createWorldBox.PackStart(_newWorldName, true, false, 0);
            _createWorldBox.PackStart(_newWorldSeed, true, false, 0);
            var newWorldHbox = new HBox();
            newWorldHbox.PackStart(_newWorldCommit, true, false, 0);
            newWorldHbox.PackStart(_newWorldCancel, true, false, 0);
            _createWorldBox.PackStart(newWorldHbox, true, false, 0);

            this.PackStart(_singleplayerLabel, true, false, 0);
            this.PackStart(_worldListView, true, false, 0);
            this.PackStart(createDeleteHbox, true, false, 0);
            this.PackStart(_playButton, true, false, 0);
            this.PackStart(_createWorldBox, true, false, 0);
            this.PackStart(_progressLabel, true, false, 0);
            this.PackStart(_progressBar, true, false, 0);
            this.PackEnd(_backButton, true, false, 0);
        }

        private static void AddWorldColumns(TreeView worldView)
        {
            CellRendererText rendererText = new CellRendererText();
            TreeViewColumn column = new TreeViewColumn("Name", rendererText, "text", 0);
            column.SortColumnId = 0;
            worldView.AppendColumn(column);
        }

        private void DeleteButton_Clicked(object? sender, EventArgs e)
        {
            Cursor origCursor = _window.Window.Cursor;
            try
            {
                _window.Window.Cursor = new Cursor(CursorType.Watch);

                TreeIter iter;
                ITreeModel model;
                _worldListView.Selection.GetSelected(out model, out iter);

                string worldName = (string)model.GetValue(iter, 0);
                WorldInfo worldInfo = (WorldInfo)model.GetValue(iter, 1);

                // Remove the World from the UI
                _worldListStore.Remove(ref iter);

                // Remove the world from the list of Worlds
                _worlds.Remove(worldInfo.Directory);

                // Remove the World from disk
                Directory.Delete(System.IO.Path.Combine(_worlds.BaseDirectory, worldInfo.Directory), true);
            }
            finally
            {
                _window.Window.Cursor = origCursor;
            }
        }

        private void PlayButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                TreeIter iter;
                _worldListView.Selection.GetSelected(out iter);
                //string worldName = (string)_worldListStore.GetValue(iter, 0);
                WorldInfo worldInfo = (WorldInfo)_worldListStore.GetValue(iter, 1);

                Discover.DoDiscovery(new Discover());
                MultiplayerServer _server = MultiplayerServer.Get();
                TrueCraft.Program.ServiceLocator = new ServiceLocater(_server, BlockRepository.Get(), ItemRepository.Get());
                TrueCraft.World.IWorld world = TrueCraft.World.World.LoadWorld(TrueCraft.Program.ServiceLocator, worldInfo.Directory);
                _server.World = world;
                TrueCraft.Program.ServerConfiguration = new ServerConfiguration()
                {
                    MOTD = null,
                    Singleplayer = true
                };

                _playButton.Sensitive = _backButton.Sensitive = _createWorldButton.Sensitive =
                    _createWorldBox.Visible = _worldListView.Sensitive = false;
                _progressBar.Visible = _progressLabel.Visible = true;
                Task.Factory.StartNew(() =>
                {
                // TODO: What if the player exitted the game from another dimension?
                IDimensionServer overWorld = (IDimensionServer)world[Core.World.DimensionID.Overworld];
                    GlobalChunkCoordinates spawnChunk = new GlobalChunkCoordinates(0, 0);
                    overWorld.Initialize(spawnChunk, _server, (value, stage) =>
                        Application.Invoke((sender, e) =>
                        {
                            _progressLabel.Text = stage;
                            _progressBar.Fraction = value;
                        }));
                    _server.Start(new IPEndPoint(IPAddress.Loopback, 0));
                    Application.Invoke((sender, e) =>
                    {
                        _playButton.Sensitive = _backButton.Sensitive = _createWorldButton.Sensitive = _worldListView.Sensitive = true;
                        var process = new Process();

                        string clientLocation = Assembly.GetExecutingAssembly().Location;
                        clientLocation = System.IO.Path.GetDirectoryName(clientLocation);
                        clientLocation = System.IO.Path.Combine(clientLocation, "TrueCraft.Client.dll");

                        string launchParams = string.Format("{0} {1} {2} {3}", clientLocation, _server.EndPoint, _window.User.Username, _window.User.SessionId);

                        process.StartInfo = new ProcessStartInfo($"dotnet",
                                 launchParams);
                        process.StartInfo.UseShellExecute = false;
                        process.EnableRaisingEvents = true;
                        process.Exited += (s, a) => Application.Invoke((s, a) =>
                        {
                            _progressBar.Visible = _progressLabel.Visible = false;
                            _window.Show();
                            _server.Stop();
                        });
                        process.Start();
                        _window.Hide();
                    });
                }).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Application.Invoke((sender, e) =>
                        {
                            using (MessageDialog msg = new MessageDialog(_window,
                                 DialogFlags.DestroyWithParent | DialogFlags.Modal,
                                 MessageType.Error,
                                 ButtonsType.Close,
                                 "Error loading world",
                                 Array.Empty<object>()))
                            {
                                msg.SecondaryText = "It's possible that this world is corrupted.";
                                msg.Run();
                            }

                            _progressBar.Visible = _progressLabel.Visible = false;
                            _playButton.Sensitive = _backButton.Sensitive = _createWorldButton.Sensitive =
                                _worldListView.Sensitive = true;
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                using (MessageDialog msg = new MessageDialog(_window, DialogFlags.DestroyWithParent | DialogFlags.DestroyWithParent,
                        MessageType.Error, ButtonsType.Close,
                        ex.Message + "\n" + ex.StackTrace, Array.Empty<object>()))
                    msg.Run();
            }
        }

        private void NewWorldCommit_Clicked(object sender, EventArgs e)
        {
            WorldInfo world = _worlds.CreateNewWorld(_newWorldName.Text, _newWorldSeed.Text);
            _createWorldBox.Visible = false;

            TreeIter row = _worldListStore.Append();
            _worldListStore.SetValue(row, 0, world.Name);
            _worldListStore.SetValue(row, 1, world);
        }
    }
}
