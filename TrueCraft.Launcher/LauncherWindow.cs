using System;
using System.Diagnostics;
using System.Reflection;
using Gtk;
using TrueCraft.Launcher.Views;
using TrueCraft.Core;

namespace TrueCraft.Launcher
{
    public class LauncherWindow : ApplicationWindow
    {
        public TrueCraftUser User { get; set; }

        private HBox _mainContainer;
        private ScrolledWindow _webScrollView;

        // TODO Change from Label to a Web Browser
        public Label WebView { get; set; }

        private LoginView _loginView;
        private int _indexLoginView;

        private MainMenuView _mainMenuView;
        private int _indexMainMenuView;

        private OptionView _optionView;
        private int _indexOptionView;

        private MultiplayerView _multiplayerView;
        private int _indexMultiplayerView;

        private SingleplayerView _singleplayerView;
        private int _indexSingleplayerView;

        private Box _interactionBox;
        private Image _trueCraftLogoImage;

        private Notebook _notebook;

        public LauncherWindow(Application app) : base(app)
        {
            this.Title = "TrueCraft Launcher";
            this.DefaultSize = new Gdk.Size(1200, 576);
            this.User = new TrueCraftUser();

            _mainContainer = new HBox();
            _webScrollView = new ScrolledWindow();
            WebView = new Label("https://truecraft.io/updates");

            _loginView = new LoginView(this);
            _mainMenuView = new MainMenuView(this);
            _optionView = new OptionView(this);
            _multiplayerView = new MultiplayerView(this);
            _singleplayerView = new SingleplayerView(this);

            _notebook = new Notebook();
            _notebook.PopupDisable();
            _notebook.ShowTabs = false;
            _indexLoginView = _notebook.AppendPage(_loginView, null);
            _indexMainMenuView = _notebook.AppendPage(_mainMenuView, null);
            _notebook.Page = _indexMainMenuView;
            _indexOptionView = _notebook.AppendPage(_optionView, null);
            _indexMultiplayerView = _notebook.AppendPage(_multiplayerView, null);
            _indexSingleplayerView = _notebook.AppendPage(_singleplayerView, null);
            
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TrueCraft.Launcher.Content.truecraft_logo.png"))
                _trueCraftLogoImage = new Image(new Gdk.Pixbuf(stream, 350, 75));

            _interactionBox = new Box(Orientation.Vertical, 1);
            _interactionBox.PackStart(_trueCraftLogoImage, true, false, 0);
            _interactionBox.PackEnd(_notebook, false, false, 0);

            _webScrollView.Add(WebView);
            _mainContainer.PackStart(_webScrollView, true, false, 0);
            _mainContainer.PackEnd(_interactionBox, true, false, 0);

            this.Add(_mainContainer);
            _mainContainer.ShowAll();
        }

        void ClientExited()
        {
            this.Visible = true;
        }

        public void ShowLoginView()
        {
           _notebook.Page = _indexLoginView;
        }

        public void ShowMainMenuView()
        {
           _notebook.Page = _indexMainMenuView;
        }

        public void ShowOptionView()
        {
           _notebook.Page = _indexOptionView;
        }

        public void ShowMultiplayerView()
        {
           _notebook.Page = _indexMultiplayerView;
        }

        public void ShowSinglePlayerView()
        {
           _notebook.Page = _indexSingleplayerView;
        }
    }
}
