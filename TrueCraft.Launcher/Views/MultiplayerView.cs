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

        private readonly LauncherWindow _window;
        private readonly Label _multiplayerLabel;
        private readonly Entry _serverIPEntry;
        private readonly Button _connectButton;
        private readonly Button _backButton;
        private readonly Button _addServerButton;
        private readonly Button _removeServerButton;
        private readonly VBox _serverCreationBox;
        private readonly Label _newServerLabel;
        private readonly Entry _newServerName;
        private readonly Entry _newServerAddress;
        private readonly Button _commitAddNewServer;
        private readonly Button _cancelAddNewServer;

        private TreeView _serverListView;
        private ListStore _serverListStore;

        public MultiplayerView(LauncherWindow window)
        {
            _window = window;
            this.SetSizeRequest(250, -1);

            _multiplayerLabel = new Label("Multiplayer")
            {
                Justify = Justification.Center
            };
            _serverIPEntry = new Entry()
            {
                PlaceholderText = "Server IP",
                Text = UserSettings.Local.LastIP
            };
            _connectButton = new Button("Connect");
            _backButton = new Button("Back");

            _serverListStore = new ListStore(typeof(Image), typeof(string), typeof(string));

            _serverListView = new TreeView(_serverListStore);
            _serverListView.SetSizeRequest(-1, 200);
            _serverListView.HeadersVisible = false;

            TreeSelection serverSelection = _serverListView.Selection;
            serverSelection.Mode = SelectionMode.Single;
            serverSelection.Changed += (sender, e) =>
            {
               int selectedCount = ((TreeSelection)sender!).CountSelectedRows();
               _removeServerButton!.Sensitive = (selectedCount == 1);
               _serverIPEntry.Sensitive = (selectedCount == 0);
            };

            _addServerButton = new Button("Add server");
            _removeServerButton = new Button("Remove") { Sensitive = false };
            _serverCreationBox = new VBox() { Visible = false };
            _newServerLabel = new Label("Add new server:") { Justify = Justification.Center };
            _newServerName = new Entry() { PlaceholderText = "Name" };
            _newServerAddress = new Entry() { PlaceholderText = "Address" };
            _commitAddNewServer = new Button("Add server");
            _cancelAddNewServer = new Button("Cancel");

            // TODO: restore this functionality
            //ServerIPEntry.KeyReleased += (sender, e) => 
            //{
            //    if (e.Key == Key.Return || e.Key == Key.NumPadEnter)
            //        ConnectButton_Clicked(sender, e);
            //};
            _backButton.Clicked += (sender, e) => _window.ShowMainMenuView();
            _connectButton.Clicked += ConnectButton_Clicked;
            _addServerButton.Clicked += (sender, e) => 
            {
                _addServerButton.Sensitive = false;
                _removeServerButton.Sensitive = false;
                _connectButton.Sensitive = false;
                _serverListView.Sensitive = false;
                _serverIPEntry.Sensitive = false;
                _serverCreationBox.Visible = true;
            };
            _cancelAddNewServer.Clicked += (sender, e) => 
            {
                _addServerButton.Sensitive = true;
                _removeServerButton.Sensitive = true;
                _connectButton.Sensitive = true;
                _serverListView.Sensitive = true;
                _serverIPEntry.Sensitive = true;
                _serverCreationBox.Visible = false;
            };

            _removeServerButton.Clicked += (sender, e) => 
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

            _commitAddNewServer.Clicked += (sender, e) => 
            {
                FavoriteServer server = new FavoriteServer(_newServerName.Text, _newServerAddress.Text);

                TreeIter iter = _serverListStore.Append();
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TrueCraft.Launcher.Content.default-server-icon.png"))
                   _serverListStore.SetValue(iter, (int)ServerColumns.Icon, new Image(new Gdk.Pixbuf(stream)));
                _serverListStore.SetValue(iter, (int)ServerColumns.Name, server.Name);
                _serverListStore.SetValue(iter, (int)ServerColumns.Address, server.Address);
                _serverListStore.SetValue(iter, (int)ServerColumns.Players, "TODO/50");

                // TODO: display busy cursor; surround with try/catch/finally
                UserSettings.Local.FavoriteServers = UserSettings.Local.FavoriteServers.Concat(new[] { server }).ToArray();
                UserSettings.Local.Save();

                _addServerButton.Sensitive = true;
                _removeServerButton.Sensitive = true;
                _connectButton.Sensitive = true;
                _serverListView.Sensitive = true;
                _serverIPEntry.Sensitive = true;
                _serverCreationBox.Visible = false;
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
            addServerHBox.PackStart(_addServerButton, true, false, 0);
            addServerHBox.PackStart(_removeServerButton, true, false, 0);

            var commitHBox = new HBox();
            commitHBox.PackStart(_commitAddNewServer, true, false, 0);
            commitHBox.PackStart(_cancelAddNewServer, true, false, 0);

            _serverCreationBox.PackStart(_newServerLabel, true, false, 0);
            _serverCreationBox.PackStart(_newServerName, true, false, 0);
            _serverCreationBox.PackStart(_newServerAddress, true, false, 0);
            _serverCreationBox.PackStart(commitHBox, true, false, 0);

            this.PackEnd(_backButton, true, false, 0);
            this.PackEnd(_connectButton, true, false, 0);
            this.PackStart(_multiplayerLabel, true, false, 0);
            this.PackStart(_serverIPEntry, true, false, 0);
            this.PackStart(_serverListView, true, false, 0);
            this.PackStart(addServerHBox, true, false, 0);
            this.PackStart(_serverCreationBox, true, false, 0);
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

        void ConnectButton_Clicked(object? sender, EventArgs e)
        {
            var ip = _serverIPEntry.Text;
            TreeSelection selection = _serverListView.Selection;
            if (selection.CountSelectedRows() != 0)
            {
               TreeIter row;
               ITreeModel model;
               selection.GetSelected(out model, out row);
               ip = (string)model.GetValue(row, (int)ServerColumns.Address);
            }

            string TrueCraftLaunchParams = string.Format("{0} {1} {2}", ip, _window.User.Username, _window.User.SessionId);
            var process = new Process();
            if (RuntimeInfo.IsMono)
                process.StartInfo = new ProcessStartInfo("mono", "TrueCraft.Client.exe " + TrueCraftLaunchParams);
            else
                process.StartInfo = new ProcessStartInfo("TrueCraft.Client.exe", TrueCraftLaunchParams);
            process.EnableRaisingEvents = true;
            process.Exited += (s, a) => Application.Invoke(ClientExited);
            process.Start();
            UserSettings.Local.LastIP = _serverIPEntry.Text;
            UserSettings.Local.Save();
            _window.Visible = false;
      }

        void ClientExited(object? sender, EventArgs e)
        {
            _window.Visible = true;
        }
    }
}