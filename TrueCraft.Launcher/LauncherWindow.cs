using System;
using System.Diagnostics;
using System.Reflection;
using Gtk;
using TrueCraft.Launcher.Views;
using TrueCraft.Core;

namespace TrueCraft.Launcher
{
    public class LauncherWindow : Window
    {
        public TrueCraftUser User { get; set; }

        private HBox _mainContainer;
        private ScrolledWindow _webScrollView;

        // TODO Change from Label to a Web Browser
        public Label WebView { get; set; }

        private LoginView _loginView;
        public MainMenuView MainMenuView { get; set; }
        public OptionView OptionView { get; set; }
        public MultiplayerView MultiplayerView { get; set; }
        public SingleplayerView SingleplayerView { get; set; }
        public VBox InteractionBox { get; set; }
        private Image _trueCraftLogoImage;

        public LauncherWindow() : base("TrueCraft Launcher")
        {
            this.DefaultSize = new Gdk.Size(1200, 576);
            this.User = new TrueCraftUser();

            _mainContainer = new HBox();
            _webScrollView = new ScrolledWindow();
            WebView = new Label("https://truecraft.io/updates");
            _loginView = new LoginView(this);
            OptionView = new OptionView(this);
            MultiplayerView = new MultiplayerView(this);
            SingleplayerView = new SingleplayerView(this);
            InteractionBox = new VBox();
            
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TrueCraft.Launcher.Content.truecraft_logo.png"))
                _trueCraftLogoImage = new Image(new Gdk.Pixbuf(stream, 350, 75));

            _webScrollView.Add(WebView);
            _mainContainer.PackStart(_webScrollView, true, false, 0);
            InteractionBox.PackStart(_trueCraftLogoImage, true, false, 0);
            InteractionBox.PackEnd(_loginView, true, false, 0);
            _mainContainer.PackEnd(InteractionBox, true, false, 0);

            this.Add(_mainContainer);
            _mainContainer.ShowAll();
        }

        void ClientExited()
        {
            this.Visible = true;
        }
    }
}
