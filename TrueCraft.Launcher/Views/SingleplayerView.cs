using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gtk;
using TrueCraft.Core;
using TrueCraft.Launcher.Singleplayer;

namespace TrueCraft.Launcher.Views
{
    public class SingleplayerView : VBox
    {
        public LauncherWindow Window { get; set; }
        public Label SingleplayerLabel { get; set; }

        private TreeView _worldListView;
        private ListStore _worldListStore;

        public Button CreateWorldButton { get; set; }
        public Button DeleteWorldButton { get; set; }
        public Button PlayButton { get; set; }
        public Button BackButton { get; set; }
        public VBox CreateWorldBox { get; set; }
        public Entry NewWorldName { get; set; }
        public Entry NewWorldSeed { get; set; }
        public Button NewWorldCommit { get; set; }
        public Button NewWorldCancel { get; set; }
        public Label ProgressLabel { get; set; }
        public ProgressBar ProgressBar { get; set; }
        public SingleplayerServer Server { get; set; }

        public SingleplayerView(LauncherWindow window)
        {
            Worlds.Local = new Worlds();
            Worlds.Local.Load();

            Window = window;
            this.SetSizeRequest(250, -1);

            SingleplayerLabel = new Label("Singleplayer")
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
               PlayButton.Sensitive = (selectedCount == 1);
               DeleteWorldButton.Sensitive = (selectedCount == 1);
            };

            CreateWorldButton = new Button("New world");
            DeleteWorldButton = new Button("Delete") { Sensitive = false };
            PlayButton = new Button("Play") { Sensitive = false };
            BackButton = new Button("Back");
            CreateWorldBox = new VBox() { Visible = false };
            NewWorldName = new Entry() { PlaceholderText = "Name" };
            NewWorldSeed = new Entry() { PlaceholderText = "Seed (optional)" };
            NewWorldCommit = new Button("Create") { Sensitive = false };
            NewWorldCancel = new Button("Cancel");

            ProgressLabel = new Label("Loading world...") { Visible = false };
            // TODO: we have to call Pulse on the Progress Bar once in a while.
            ProgressBar = new ProgressBar() { Visible = false, Fraction = 0 };

            BackButton.Clicked += (sender, e) =>
            {
                Window.InteractionBox.Remove(this);
                Window.InteractionBox.PackEnd(Window.MainMenuView, true, false, 0);
            };
            CreateWorldButton.Clicked += (sender, e) =>
            {
                CreateWorldBox.Visible = true;
            };
            NewWorldCancel.Clicked += (sender, e) =>
            {
                CreateWorldBox.Visible = false;
            };
            NewWorldName.Changed += (sender, e) =>
            {
                NewWorldCommit.Sensitive = !string.IsNullOrEmpty(NewWorldName.Text);
            };
            NewWorldCommit.Clicked += NewWorldCommit_Clicked;

            PlayButton.Clicked += PlayButton_Clicked;
            DeleteWorldButton.Clicked += (sender, e) => 
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
            createDeleteHbox.PackStart(CreateWorldButton, true, false, 0);
            createDeleteHbox.PackStart(DeleteWorldButton, true, false, 0);

            CreateWorldBox.PackStart(NewWorldName, true, false, 0);
            CreateWorldBox.PackStart(NewWorldSeed, true, false, 0);
            var newWorldHbox = new HBox();
            newWorldHbox.PackStart(NewWorldCommit, true, false, 0);
            newWorldHbox.PackStart(NewWorldCancel, true, false, 0);
            CreateWorldBox.PackStart(newWorldHbox, true, false, 0);

            this.PackStart(SingleplayerLabel, true, false, 0);
            this.PackStart(_worldListView, true, false, 0);
            this.PackStart(createDeleteHbox, true, false, 0);
            this.PackStart(PlayButton, true, false, 0);
            this.PackStart(CreateWorldBox, true, false, 0);
            this.PackStart(ProgressLabel, true, false, 0);
            this.PackStart(ProgressBar, true, false, 0);
            this.PackEnd(BackButton, true, false, 0);
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
            TrueCraft.Core.World.World world = Worlds.Local.Saves.Where(s => s.Name != worldName).First<TrueCraft.Core.World.World>();
            Server = new SingleplayerServer(world);

            PlayButton.Sensitive = BackButton.Sensitive = CreateWorldButton.Sensitive =
                CreateWorldBox.Visible = _worldListView.Sensitive = false;
            ProgressBar.Visible = ProgressLabel.Visible = true;
            Task.Factory.StartNew(() =>
            {
                Server.Initialize((value, stage) =>
                    Application.Invoke((sender, e) =>
                    {
                        ProgressLabel.Text = stage;
                        ProgressBar.Fraction = value;
                    }));
                Server.Start();
                Application.Invoke((sender, e) =>
                {
                    PlayButton.Sensitive = BackButton.Sensitive = CreateWorldButton.Sensitive = _worldListView.Sensitive = true;
                    var launchParams = string.Format("{0} {1} {2}", Server.Server.EndPoint, Window.User.Username, Window.User.SessionId);
                    var process = new Process();
                    if (RuntimeInfo.IsMono)
                        process.StartInfo = new ProcessStartInfo("mono", "TrueCraft.Client.exe " + launchParams);
                    else
                        process.StartInfo = new ProcessStartInfo("TrueCraft.Client.exe", launchParams);
                    process.EnableRaisingEvents = true;
                    process.Exited += (s, a) => Application.Invoke((s, a) =>
                    {
                        ProgressBar.Visible = ProgressLabel.Visible = false;
                        Window.Show();
                        Window.ShowInTaskbar = true;
                        Server.Stop();
                        Server.World.Save();
                    });
                    process.Start();
                    Window.ShowInTaskbar = false;
                    Window.Hide();
                });
            }).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Application.Invoke(() =>
                    {
                        MessageDialog.ShowError("Error loading world", "It's possible that this world is corrupted.");
                        ProgressBar.Visible = ProgressLabel.Visible = false;
                        PlayButton.Sensitive = BackButton.Sensitive = CreateWorldButton.Sensitive =
                            _worldListView.Sensitive = true;
                    });
                }
            });
        }

        void NewWorldCommit_Clicked(object sender, EventArgs e)
        {
            var world = Worlds.Local.CreateNewWorld(NewWorldName.Text, NewWorldSeed.Text);
            CreateWorldBox.Visible = false;
            TreeIter row = _worldListStore.Append();
            _worldListStore.SetValue(row, 0, world.Name);
        }
    }
}
