using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Gtk;
using TrueCraft.Core;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;
using TrueCraft.Launcher.Singleplayer;

namespace TrueCraft.Launcher.Views
{
    public class SingleplayerView : VBox
    {
        private LauncherWindow _window;
        public Label _singleplayerLabel;

        private TreeView _worldListView;
        private ListStore _worldListStore;

        public Button _createWorldButton;
        public Button _deleteWorldButton;
        public Button _playButton;
        public Button _backButton;
        public VBox _createWorldBox;
        public Entry _newWorldName;
        public Entry _newWorldSeed;
        public Button _newWorldCommit;
        public Button _newWorldCancel;
        public Label _progressLabel;
        public ProgressBar _progressBar;

        public SingleplayerView(LauncherWindow window)
        {
            Worlds.Local = new Worlds();
            Worlds.Local.Load();

            _window = window;
            this.SetSizeRequest(250, -1);

            _singleplayerLabel = new Label("Singleplayer")
            {
                Justify = Justification.Center
            };

            _worldListStore = new ListStore(typeof(string));
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
            _deleteWorldButton.Clicked += (sender, e) => 
            {
                TreeIter iter;
                ITreeModel model;
                _worldListView.Selection.GetSelected(out model, out iter);

                string worldName = (string)model.GetValue(iter, 0);
                _worldListStore.Remove(ref iter);

                // TODO: display busy cursor; surround with try/catch/finally
                // TODO: Do world names have to be unique???
                Worlds.Local.Saves = Worlds.Local.Saves.Where(s => s.Name != worldName).ToArray();

                // TODO: actually delete the World
                // Directory.Delete(world.BaseDirectory, true);
            };

            foreach (var world in Worlds.Local.Saves)
            {
                TreeIter row = _worldListStore.Append();
                _worldListStore.SetValue(row, 0, world.Name);
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

        public void PlayButton_Clicked(object sender, EventArgs e)
        {
            TreeIter iter;
            _worldListView.Selection.GetSelected(out iter);
            string worldName = (string)_worldListStore.GetValue(iter, 0);
            // TODO: Do world names have to be unique?
            MultiplayerServer _server = MultiplayerServer.Get(worldName);
            TrueCraft.World.IWorld world = (World.IWorld)_server.World;
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

        void NewWorldCommit_Clicked(object sender, EventArgs e)
        {
            var world = Worlds.Local.CreateNewWorld(_newWorldName.Text, _newWorldSeed.Text);
            _createWorldBox.Visible = false;
            TreeIter row = _worldListStore.Append();
            _worldListStore.SetValue(row, 0, world.Name);
        }
    }
}
