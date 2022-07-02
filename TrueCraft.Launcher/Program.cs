using System;
using System.Threading;
using System.Net;
using Gtk;
using TrueCraft.Core;

namespace TrueCraft.Launcher
{
    class Program
    {
        private static LauncherWindow _window = null!;

        [STAThread]
        public static void Main(string[] args)
        {
            UserSettings.Local.Load();

            Application.Init();
            using (Application app = new Application("TrueCraft.Launcher", GLib.ApplicationFlags.None))
            {
                app.Register(GLib.Cancellable.Current);

                _window = new LauncherWindow(app);
                app.AddWindow(_window);
                _window.DeleteEvent += (sender, e) => Application.Quit();
                _window.Show();

                // TODO: restore Keep Session Alive for multiplayer.
                //Thread thread = new Thread(KeepSessionAlive);
                //thread.IsBackground = true;
                //thread.Priority = ThreadPriority.Lowest;
                //thread.Start();

                Application.Run();
                _window.Dispose();
            }
        }

        private static void KeepSessionAlive()
        {
            while (true)
            {
                if (!string.IsNullOrEmpty(_window.User.SessionId))
                {
                    var wc = new WebClient();
                    wc.DownloadString(string.Format(TrueCraftUser.AuthServer + "/session?name={0}&session={1}",
                        _window.User.Username, _window.User.SessionId));
                }
                Thread.Sleep(60 * 5 * 1000);
            }
        }
    }
}
