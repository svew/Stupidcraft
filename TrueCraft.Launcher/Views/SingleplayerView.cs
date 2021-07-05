﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gtk;
using TrueCraft.Launcher.Singleplayer;
using TrueCraft.Core;

namespace TrueCraft.Launcher.Views
{
    public class SingleplayerView : VBox
    {
        public LauncherWindow Window { get; set; }
        public Label SingleplayerLabel { get; set; }
        public TreeView WorldListView { get; set; }
        public Button CreateWorldButton { get; set; }
        public Button DeleteWorldButton { get; set; }
        public Button PlayButton { get; set; }
        public Button BackButton { get; set; }
        public VBox CreateWorldBox { get; set; }
        public Entry NewWorldName { get; set; }
        public Entry NewWorldSeed { get; set; }
        public Button NewWorldCommit { get; set; }
        public Button NewWorldCancel { get; set; }
        public ListStore WorldListStore { get; set; }
        public Label ProgressLabel { get; set; }
        public ProgressBar ProgressBar { get; set; }
        public SingleplayerServer Server { get; set; }
        public string NameField { get; set; }

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
            WorldListView = new ListView
            {
                MinHeight = 200,
                SelectionMode = SelectionMode.Single
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
            NameField = string.Empty;
            WorldListStore = new ListStore(NameField);
            WorldListView.DataSource = WorldListStore;
            WorldListView.HeadersVisible = false;
            WorldListView.Columns.Add(new ListViewColumn("Name", new TextCellView { TextField = NameField, Editable = false }));
            ProgressLabel = new Label("Loading world...") { Visible = false };
            ProgressBar = new ProgressBar() { Visible = false, Indeterminate = true, Fraction = 0 };

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
            WorldListView.SelectionChanged += (sender, e) => 
            {
                PlayButton.Sensitive = DeleteWorldButton.Sensitive = WorldListView.SelectedRow != -1;
            };
            PlayButton.Clicked += PlayButton_Clicked;
            DeleteWorldButton.Clicked += (sender, e) => 
            {
                var world = Worlds.Local.Saves[WorldListView.SelectedRow];
                WorldListStore.RemoveRow(WorldListView.SelectedRow);
                Worlds.Local.Saves = Worlds.Local.Saves.Where(s => s != world).ToArray();
                Directory.Delete(world.BaseDirectory, true);
            };

            foreach (var world in Worlds.Local.Saves)
            {
                var row = WorldListStore.AddRow();
                WorldListStore.SetValue(row, NameField, world.Name);
            }

            var createDeleteHbox = new HBox();
            CreateWorldButton.WidthRequest = DeleteWorldButton.WidthRequest = 0.5;
            createDeleteHbox.PackStart(CreateWorldButton, true, false, 0);
            createDeleteHbox.PackStart(DeleteWorldButton, true, false, 0);

            CreateWorldBox.PackStart(NewWorldName, true, false, 0);
            CreateWorldBox.PackStart(NewWorldSeed, true, false, 0);
            var newWorldHbox = new HBox();
            NewWorldCommit.WidthRequest = NewWorldCancel.WidthRequest = 0.5;
            newWorldHbox.PackStart(NewWorldCommit, true, false, 0);
            newWorldHbox.PackStart(NewWorldCancel, true, false, 0);
            CreateWorldBox.PackStart(newWorldHbox, true, false, 0);

            this.PackStart(SingleplayerLabel, true, false, 0);
            this.PackStart(WorldListView, true, false, 0);
            this.PackStart(createDeleteHbox, true, false, 0);
            this.PackStart(PlayButton, true, false, 0);
            this.PackStart(CreateWorldBox, true, false, 0);
            this.PackStart(ProgressLabel, true, false, 0);
            this.PackStart(ProgressBar, true, false, 0);
            this.PackEnd(BackButton, true, false, 0);
        }

        public void PlayButton_Clicked(object sender, EventArgs e)
        {
            Server = new SingleplayerServer(Worlds.Local.Saves[WorldListView.SelectedRow]);
            PlayButton.Sensitive = BackButton.Sensitive = CreateWorldButton.Sensitive =
                CreateWorldBox.Visible = WorldListView.Sensitive = false;
            ProgressBar.Visible = ProgressLabel.Visible = true;
            Task.Factory.StartNew(() =>
            {
                Server.Initialize((value, stage) =>
                    Application.Invoke(() =>
                    {
                        ProgressBar.Indeterminate = false;
                        ProgressLabel.Text = stage;
                        ProgressBar.Fraction = value;
                    }));
                Server.Start();
                Application.Invoke(() =>
                {
                    PlayButton.Sensitive = BackButton.Sensitive = CreateWorldButton.Sensitive = WorldListView.Sensitive = true;
                    var launchParams = string.Format("{0} {1} {2}", Server.Server.EndPoint, Window.User.Username, Window.User.SessionId);
                    var process = new Process();
                    if (RuntimeInfo.IsMono)
                        process.StartInfo = new ProcessStartInfo("mono", "TrueCraft.Client.exe " + launchParams);
                    else
                        process.StartInfo = new ProcessStartInfo("TrueCraft.Client.exe", launchParams);
                    process.EnableRaisingEvents = true;
                    process.Exited += (s, a) => Application.Invoke(() =>
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
                            WorldListView.Sensitive = true;
                    });
                }
            });
        }

        void NewWorldCommit_Clicked(object sender, EventArgs e)
        {
            var world = Worlds.Local.CreateNewWorld(NewWorldName.Text, NewWorldSeed.Text);
            CreateWorldBox.Visible = false;
            var row = WorldListStore.AddRow();
            WorldListStore.SetValue(row, NameField, world.Name);
        }
    }
}