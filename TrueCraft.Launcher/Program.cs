using System;
using System.Threading;
using System.Net;
using Gtk;
using TrueCraft.Core;

namespace TrueCraft.Launcher
{
    class Program
    {
        public static LauncherWindow Window { get; set; }

        [STAThread]
        public static void Main(string[] args)
        {
            UserSettings.Local = new UserSettings();
            UserSettings.Local.Load();

            Application.Init();
            using (Application app = new Application("TrueCraft.Launcher", GLib.ApplicationFlags.None))
            {
                app.Register(GLib.Cancellable.Current);

                Window = new LauncherWindow(app);
                app.AddWindow(Window);
                Window.DeleteEvent += (sender, e) => Application.Quit();
                Window.Show();

                // TODO: restore Keep Session Alive for multiplayer.
                //Thread thread = new Thread(KeepSessionAlive);
                //thread.IsBackground = true;
                //thread.Priority = ThreadPriority.Lowest;
                //thread.Start();

                Application.Run();
                Window.Dispose();
            }
        }

        private static void KeepSessionAlive()
        {
            while (true)
            {
                if (!string.IsNullOrEmpty(Window.User.SessionId))
                {
                    var wc = new WebClient();
                    wc.DownloadString(string.Format(TrueCraftUser.AuthServer + "/session?name={0}&session={1}",
                        Window.User.Username, Window.User.SessionId));
                }
                Thread.Sleep(60 * 5 * 1000);
            }
        }
    }
}
