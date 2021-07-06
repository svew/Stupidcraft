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
            Application.Init();

            UserSettings.Local = new UserSettings();
            UserSettings.Local.Load();

            var thread = new Thread(KeepSessionAlive);
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.Lowest;
            Window = new LauncherWindow();
            thread.Start();
            Window.Show();
            // TODO Check if closing the Window closes the Application
            // Window.Closed += (sender, e) => Application.Exit();
            Application.Run();
            Window.Dispose();
            thread.Abort();
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
