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

        public HBox MainContainer { get; set; }
        public Layout WebScrollView { get; set; }
        public WebView WebView { get; set; }

        public LoginView LoginView { get; set; }
        public MainMenuView MainMenuView { get; set; }
        public OptionView OptionView { get; set; }
        public MultiplayerView MultiplayerView { get; set; }
        public SingleplayerView SingleplayerView { get; set; }
        public VBox InteractionBox { get; set; }
        public Image TrueCraftLogoImage { get; set; }

        public LauncherWindow()
        {
            this.Title = "TrueCraft Launcher";
            this.Width = 1200;
            this.Height = 576;
            this.User = new TrueCraftUser();

            MainContainer = new HBox();
            WebScrollView = new Layout();
            WebView = new WebView("https://truecraft.io/updates");
            LoginView = new LoginView(this);
            OptionView = new OptionView(this);
            MultiplayerView = new MultiplayerView(this);
            SingleplayerView = new SingleplayerView(this);
            InteractionBox = new VBox();
            
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TrueCraft.Launcher.Content.truecraft_logo.png"))
                TrueCraftLogoImage = new Image(Image.FromStream(stream).WithBoxSize(350, 75));

            WebScrollView.Content = WebView;
            MainContainer.PackStart(WebScrollView, true);
            InteractionBox.PackStart(TrueCraftLogoImage);
            InteractionBox.PackEnd(LoginView);
            MainContainer.PackEnd(InteractionBox);

            this.Content = MainContainer;
        }

        void ClientExited()
        {
            this.Show();
            this.ShowInTaskbar = true;
        }
    }
}
