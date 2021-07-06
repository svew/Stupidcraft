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
        public LauncherWindow Window { get; set; }

        public Label MultiplayerLabel { get; set; }
        public Entry ServerIPEntry { get; set; }
        public Button ConnectButton { get; set; }
        public Button BackButton { get; set; }
        public Button AddServerButton { get; set; }
        public Button RemoveServerButton { get; set; }
        public TreeView ServerListView { get; set; }
        public VBox ServerCreationBox { get; set; }
        public Label NewServerLabel { get; set; }
        public Entry NewServerName { get; set; }
        public Entry NewServerAddress { get; set; }
        public Button CommitAddNewServer { get; set; }
        public Button CancelAddNewServer { get; set; }
        public ListStore ServerListStore { get; set; }

        public MultiplayerView(LauncherWindow window)
        {
            Window = window;
            this.SetSiteRequest(250, -1);

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
            ServerListView = new TreeView() { SelectionMode = SelectionMode.Single };
            ServerListView.SetSizeRequest(-1, 200);
            AddServerButton = new Button("Add server");
            RemoveServerButton = new Button("Remove") { Sensitive = false };
            ServerCreationBox = new VBox() { Visible = false };
            NewServerLabel = new Label("Add new server:") { Justify = Justification.Center };
            NewServerName = new Entry() { PlaceholderText = "Name" };
            NewServerAddress = new Entry() { PlaceholderText = "Address" };
            CommitAddNewServer = new Button("Add server");
            CancelAddNewServer = new Button("Cancel");

            var iconField = new DataField<Image>();
            var nameField = new DataField<string>();
            var playersField = new DataField<string>();
            ServerListStore = new ListStore(iconField, nameField, playersField);
            ServerListView.DataSource = ServerListStore;
            ServerListView.HeadersVisible = false;
            ServerListView.Columns.Add(new ListViewColumn("Icon", new ImageCellView { ImageField = iconField }));
            ServerListView.Columns.Add(new ListViewColumn("Name", new TextCellView { TextField = nameField }));
            ServerListView.Columns.Add(new ListViewColumn("Players", new TextCellView { TextField = playersField }));

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
            ServerListView.SelectionChanged += (sender, e) => 
            {
                RemoveServerButton.Sensitive = ServerListView.SelectedRow != -1;
                ServerIPEntry.Sensitive = ServerListView.SelectedRow == -1;
            };
            AddServerButton.Clicked += (sender, e) => 
            {
                AddServerButton.Sensitive = false;
                RemoveServerButton.Sensitive = false;
                ConnectButton.Sensitive = false;
                ServerListView.Sensitive = false;
                ServerIPEntry.Sensitive = false;
                ServerCreationBox.Visible = true;
            };
            CancelAddNewServer.Clicked += (sender, e) => 
            {
                AddServerButton.Sensitive = true;
                RemoveServerButton.Sensitive = true;
                ConnectButton.Sensitive = true;
                ServerListView.Sensitive = true;
                ServerIPEntry.Sensitive = true;
                ServerCreationBox.Visible = false;
            };
            RemoveServerButton.Clicked += (sender, e) => 
            {
                var server = UserSettings.Local.FavoriteServers[ServerListView.SelectedRow];
                ServerListStore.RemoveRow(ServerListView.SelectedRow);
                UserSettings.Local.FavoriteServers = UserSettings.Local.FavoriteServers.Where(
                    s => s.Name != server.Name && s.Address != server.Address).ToArray();
                UserSettings.Local.Save();
            };
            CommitAddNewServer.Clicked += (sender, e) => 
            {
                var server = new FavoriteServer
                {
                    Name = NewServerName.Text,
                    Address = NewServerAddress.Text
                };
                var row = ServerListStore.AddRow();
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TrueCraft.Launcher.Content.default-server-icon.png"))
                    ServerListStore.SetValue(row, iconField, new Image(new GdkPixbuf(stream)));
                ServerListStore.SetValue(row, nameField, server.Name);
                ServerListStore.SetValue(row, playersField, "TODO/50");
                UserSettings.Local.FavoriteServers = UserSettings.Local.FavoriteServers.Concat(new[] { server }).ToArray();
                UserSettings.Local.Save();
                AddServerButton.Sensitive = true;
                RemoveServerButton.Sensitive = true;
                ConnectButton.Sensitive = true;
                ServerListView.Sensitive = true;
                ServerIPEntry.Sensitive = true;
                ServerCreationBox.Visible = false;
            };

            foreach (var server in UserSettings.Local.FavoriteServers)
            {
                var row = ServerListStore.AddRow();
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TrueCraft.Launcher.Content.default-server-icon.png"))
                    ServerListStore.SetValue(row, iconField, new Image(new GdkPixbuf(stream)));
                ServerListStore.SetValue(row, nameField, server.Name);
                ServerListStore.SetValue(row, playersField, "TODO/50");
            }

            var addServerHBox = new HBox();
            AddServerButton.WidthRequest = RemoveServerButton.WidthRequest = 0.5;
            addServerHBox.PackStart(AddServerButton, true, false, 0);
            addServerHBox.PackStart(RemoveServerButton, true, false, 0);

            var commitHBox = new HBox();
            CancelAddNewServer.WidthRequest = CommitAddNewServer.WidthRequest = 0.5;
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
            this.PackStart(ServerListView, true, false, 0);
            this.PackStart(addServerHBox, true, false, 0);
            this.PackStart(ServerCreationBox, true, false, 0);
        }

        void ConnectButton_Clicked(object sender, EventArgs e)
        {
            var ip = ServerIPEntry.Text;
            if (ServerListView.SelectedRow != -1)
                ip = UserSettings.Local.FavoriteServers[ServerListView.SelectedRow].Address;
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