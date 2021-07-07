using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Gtk;
using TrueCraft.Core;

namespace TrueCraft.Launcher.Views
{
    public class MultiplayerView : VBox
    {
        private enum ServerColumns
        {
           Icon,
           Name,
           Address,
           Players
        }


        public LauncherWindow Window { get; set; }

        public Label MultiplayerLabel { get; set; }
        public Entry ServerIPEntry { get; set; }
        public Button ConnectButton { get; set; }
        public Button BackButton { get; set; }
        public Button AddServerButton { get; set; }
        public Button RemoveServerButton { get; set; }
        public VBox ServerCreationBox { get; set; }
        public Label NewServerLabel { get; set; }
        public Entry NewServerName { get; set; }
        public Entry NewServerAddress { get; set; }
        public Button CommitAddNewServer { get; set; }
        public Button CancelAddNewServer { get; set; }

        private TreeView _serverListView;
        private ListStore _serverListStore;

        public MultiplayerView(LauncherWindow window)
        {
            Window = window;
            this.SetSizeRequest(250, -1);

            MultiplayerLabel = new Label("Multiplayer")
            {
                Justify = Justification.Center
            };
            ServerIPEntry = new Entry()
            {
                PlaceholderText = "Server IP",
                Text = UserSettings.Local.LastIP
            };
            ConnectButton = new Button("Connect");
            BackButton = new Button("Back");

            _serverListStore = new ListStore(typeof(Image), typeof(string), typeof(string));

            _serverListView = new TreeView(_serverListStore);
            _serverListView.SetSizeRequest(-1, 200);
            _serverListView.HeadersVisible = false;

            TreeSelection serverSelection = _serverListView.Selection;
            serverSelection.Mode = SelectionMode.Single;
            serverSelection.Changed += (sender, e) =>
            {
               int selectedCount = ((TreeSelection)sender).CountSelectedRows();
               RemoveServerButton.Sensitive = (selectedCount == 1);
               ServerIPEntry.Sensitive = (selectedCount == 0);
            };

            AddServerButton = new Button("Add server");
            RemoveServerButton = new Button("Remove") { Sensitive = false };
            ServerCreationBox = new VBox() { Visible = false };
            NewServerLabel = new Label("Add new server:") { Justify = Justification.Center };
            NewServerName = new Entry() { PlaceholderText = "Name" };
            NewServerAddress = new Entry() { PlaceholderText = "Address" };
            CommitAddNewServer = new Button("Add server");
            CancelAddNewServer = new Button("Cancel");

            // TODO: restore this functionality
            //ServerIPEntry.KeyReleased += (sender, e) => 
            //{
            //    if (e.Key == Key.Return || e.Key == Key.NumPadEnter)
            //        ConnectButton_Clicked(sender, e);
            //};
            BackButton.Clicked += (sender, e) =>
            {
                Window.InteractionBox.Remove(this);
                Window.InteractionBox.PackEnd(Window.MainMenuView, true, false, 0);
            };
            ConnectButton.Clicked += ConnectButton_Clicked;
            AddServerButton.Clicked += (sender, e) => 
            {
                AddServerButton.Sensitive = false;
                RemoveServerButton.Sensitive = false;
                ConnectButton.Sensitive = false;
                _serverListView.Sensitive = false;
                ServerIPEntry.Sensitive = false;
                ServerCreationBox.Visible = true;
            };
            CancelAddNewServer.Clicked += (sender, e) => 
            {
                AddServerButton.Sensitive = true;
                RemoveServerButton.Sensitive = true;
                ConnectButton.Sensitive = true;
                _serverListView.Sensitive = true;
                ServerIPEntry.Sensitive = true;
                ServerCreationBox.Visible = false;
            };

            RemoveServerButton.Clicked += (sender, e) => 
            {
                TreeIter iter;
                ITreeModel model;
                _serverListView.Selection.GetSelected(out model, out iter);
                string serverName = (string)model.GetValue(iter, (int)ServerColumns.Name);
                string serverAddress = (string)model.GetValue(iter, (int)ServerColumns.Address);
                _serverListStore.Remove(ref iter);

                // TODO: display busy cursor; surround with try/catch/finally
                UserSettings.Local.FavoriteServers = UserSettings.Local.FavoriteServers.Where(
                    s => s.Name != serverName && s.Address != serverAddress).ToArray();
                UserSettings.Local.Save();
            };

            CommitAddNewServer.Clicked += (sender, e) => 
            {
                var server = new FavoriteServer
                {
                    Name = NewServerName.Text,
                    Address = NewServerAddress.Text
                };

                TreeIter iter = _serverListStore.Append();
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TrueCraft.Launcher.Content.default-server-icon.png"))
                   _serverListStore.SetValue(iter, (int)ServerColumns.Icon, new Image(new Gdk.Pixbuf(stream)));
                _serverListStore.SetValue(iter, (int)ServerColumns.Name, server.Name);
                _serverListStore.SetValue(iter, (int)ServerColumns.Address, server.Address);
                _serverListStore.SetValue(iter, (int)ServerColumns.Players, "TODO/50");

                // TODO: display busy cursor; surround with try/catch/finally
                UserSettings.Local.FavoriteServers = UserSettings.Local.FavoriteServers.Concat(new[] { server }).ToArray();
                UserSettings.Local.Save();

                AddServerButton.Sensitive = true;
                RemoveServerButton.Sensitive = true;
                ConnectButton.Sensitive = true;
                _serverListView.Sensitive = true;
                ServerIPEntry.Sensitive = true;
                ServerCreationBox.Visible = false;
            };

            foreach (var server in UserSettings.Local.FavoriteServers)
            {
                TreeIter row = _serverListStore.Append();
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TrueCraft.Launcher.Content.default-server-icon.png"))
                    _serverListStore.SetValue(row, (int)ServerColumns.Icon, new Image(new Gdk.Pixbuf(stream)));
                _serverListStore.SetValue(row, (int)ServerColumns.Name, server.Name);
                _serverListStore.SetValue(row, (int)ServerColumns.Address, server.Address);
                _serverListStore.SetValue(row, (int)ServerColumns.Players, "TODO/50");
            }

            var addServerHBox = new HBox();
            addServerHBox.PackStart(AddServerButton, true, false, 0);
            addServerHBox.PackStart(RemoveServerButton, true, false, 0);

            var commitHBox = new HBox();
            commitHBox.PackStart(CommitAddNewServer, true, false, 0);
            commitHBox.PackStart(CancelAddNewServer, true, false, 0);

            ServerCreationBox.PackStart(NewServerLabel, true, false, 0);
            ServerCreationBox.PackStart(NewServerName, true, false, 0);
            ServerCreationBox.PackStart(NewServerAddress, true, false, 0);
            ServerCreationBox.PackStart(commitHBox, true, false, 0);

            this.PackEnd(BackButton, true, false, 0);
            this.PackEnd(ConnectButton, true, false, 0);
            this.PackStart(MultiplayerLabel, true, false, 0);
            this.PackStart(ServerIPEntry, true, false, 0);
            this.PackStart(_serverListView, true, false, 0);
            this.PackStart(addServerHBox, true, false, 0);
            this.PackStart(ServerCreationBox, true, false, 0);
        }

      private void AddServerColumns()
      {
         // Server Icon Column
         CellRendererPixbuf imageRenderer = new CellRendererPixbuf();
         TreeViewColumn column = new TreeViewColumn(String.Empty, imageRenderer,
                  "icon", ServerColumns.Icon);
         column.SortColumnId = (int)ServerColumns.Icon;
         _serverListView.AppendColumn(column);

         // Server Name column
         CellRendererText rendererText = new CellRendererText();
         column = new TreeViewColumn("Name", rendererText, "text", ServerColumns.Name);
         column.SortColumnId = (int)ServerColumns.Name;
         _serverListView.AppendColumn(column);

         // Server Address Column
         rendererText = new CellRendererText();
         column = new TreeViewColumn("Address", rendererText, "text", ServerColumns.Address);
         column.SortColumnId = (int)ServerColumns.Address;
         column.Visible = false;
         _serverListView.AppendColumn(column);

         // Players Column
         rendererText = new CellRendererText();
         column = new TreeViewColumn("Players", rendererText, "text", ServerColumns.Players);
         column.SortColumnId = (int)ServerColumns.Players;
         _serverListView.AppendColumn(column);
      }

        void ConnectButton_Clicked(object sender, EventArgs e)
        {
            var ip = ServerIPEntry.Text;
            TreeSelection selection = _serverListView.Selection;
            if (selection.CountSelectedRows() != 0)
            {
               TreeIter row;
               ITreeModel model;
               selection.GetSelected(out model, out row);
               ip = (string)model.GetValue(row, (int)ServerColumns.Address);
            }

            string TrueCraftLaunchParams = string.Format("{0} {1} {2}", ip, Window.User.Username, Window.User.SessionId);
            var process = new Process();
            if (RuntimeInfo.IsMono)
                process.StartInfo = new ProcessStartInfo("mono", "TrueCraft.Client.exe " + TrueCraftLaunchParams);
            else
                process.StartInfo = new ProcessStartInfo("TrueCraft.Client.exe", TrueCraftLaunchParams);
            process.EnableRaisingEvents = true;
            process.Exited += (s, a) => Application.Invoke(ClientExited);
            process.Start();
            UserSettings.Local.LastIP = ServerIPEntry.Text;
            UserSettings.Local.Save();
            Window.Visible = false;
      }

        void ClientExited()
        {
            Window.Visible = true;
        }
    }
}