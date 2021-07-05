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
               Justify = Justification.Center
            };
            SingleplayerButton = new Button("Singleplayer");
            MultiplayerButton = new Button("Multiplayer");
            OptionsButton = new Button("Options");
            QuitButton = new Button("Quit Game");

            SingleplayerButton.Clicked += (sender, e) =>
            {
                Window.InteractionBox.Remove(this);
                Window.InteractionBox.PackEnd(Window.SingleplayerView, true, false, 0);
            };
            MultiplayerButton.Clicked += (sender, e) =>
            {
                Window.InteractionBox.Remove(this);
                Window.InteractionBox.PackEnd(Window.MultiplayerView, true, false, 0);
            };
            OptionsButton.Clicked += (sender, e) =>
            {
                Window.InteractionBox.Remove(this);
                window.InteractionBox.PackEnd(Window.OptionView, true, false, 0);
            };
            QuitButton.Clicked += (sender, e) => Application.Exit();

            this.PackStart(WelcomeText, true, false, 0);
            this.PackStart(SingleplayerButton, true, false, 0);
            this.PackStart(MultiplayerButton, true, false, 0);
            this.PackStart(OptionsButton, true, false, 0);
            this.PackEnd(QuitButton, true, false, 0);
        }
    }
}