using System;
using Gtk;

namespace TrueCraft.Launcher.Views
{
    public class MainMenuView : VBox
    {
        public LauncherWindow Window { get; set; }

        public Label WelcomeText { get; set; }
        public Button SingleplayerButton { get; set; }
        public Button MultiplayerButton { get; set; }
        public Button OptionsButton { get; set; }
        public Button QuitButton { get; set; }

        public MainMenuView(LauncherWindow window)
        {
            Window = window;
            this.SetSizeRequest(250, -1);

            WelcomeText = new Label("Welcome, " + Window.User.Username)
            {
                TextAlignment = Alignment.Center
            };
            SingleplayerButton = new Button("Singleplayer");
            MultiplayerButton = new Button("Multiplayer");
            OptionsButton = new Button("Options");
            QuitButton = new Button("Quit Game");

            SingleplayerButton.Clicked += (sender, e) =>
            {
                Window.InteractionBox.Remove(this);
                Window.InteractionBox.PackEnd(Window.SingleplayerView);
            };
            MultiplayerButton.Clicked += (sender, e) =>
            {
                Window.InteractionBox.Remove(this);
                Window.InteractionBox.PackEnd(Window.MultiplayerView);
            };
            OptionsButton.Clicked += (sender, e) =>
            {
                Window.InteractionBox.Remove(this);
                window.InteractionBox.PackEnd(Window.OptionView);
            };
            QuitButton.Clicked += (sender, e) => Application.Exit();

            this.PackStart(WelcomeText);
            this.PackStart(SingleplayerButton);
            this.PackStart(MultiplayerButton);
            this.PackStart(OptionsButton);
            this.PackEnd(QuitButton);
        }
    }
}