using System;
using Gtk;

namespace TrueCraft.Launcher.Views
{
    public class MainMenuView : VBox
    {
        private LauncherWindow _launcher { get; set; }

        private Label _welcomeText;
        private Button _singleplayerButton;
        private Button _multiplayerButton;
        private Button _optionsButton;
        private Button _quitButton;

        public MainMenuView(LauncherWindow window)
        {
            _launcher = window;
            this.SetSizeRequest(250, -1);

            _welcomeText = new Label("Welcome, " + _launcher.User.Username)
            {
               Justify = Justification.Center
            };
            _singleplayerButton = new Button("Singleplayer");
            _multiplayerButton = new Button("Multiplayer");
            _optionsButton = new Button("Options");
            _quitButton = new Button("Quit Game");

            _singleplayerButton.Clicked += (sender, e) =>
            {
                _launcher.InteractionBox.Remove(this);
                _launcher.InteractionBox.PackEnd(_launcher.SingleplayerView, true, false, 0);
            };
            _multiplayerButton.Clicked += (sender, e) =>
            {
                _launcher.InteractionBox.Remove(this);
                _launcher.InteractionBox.PackEnd(_launcher.MultiplayerView, true, false, 0);
            };
            _optionsButton.Clicked += (sender, e) =>
            {
                _launcher.InteractionBox.Remove(this);
                window.InteractionBox.PackEnd(_launcher.OptionView, true, false, 0);
            };
            _quitButton.Clicked += (sender, e) => Application.Quit();

            this.PackStart(_welcomeText, true, false, 0);
            this.PackStart(_singleplayerButton, true, false, 0);
            this.PackStart(_multiplayerButton, true, false, 0);
            this.PackStart(_optionsButton, true, false, 0);
            this.PackEnd(_quitButton, true, false, 0);
        }
    }
}