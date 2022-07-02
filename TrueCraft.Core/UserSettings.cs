using System;
using Newtonsoft.Json;
using System.IO;

namespace TrueCraft.Core
{
    public class UserSettings
    {
        public static UserSettings Local { get; set; }

        public bool AutoLogin { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string LastIP { get; set; }
        public string SelectedTexturePack { get; set; }
        public FavoriteServer[] FavoriteServers { get; set; }
        public bool IsFullscreen { get; set; }
        public bool InvertedMouse { get; set; }
        public WindowResolution WindowResolution { get; set; }

        public UserSettings()
        {
            AutoLogin = false;
            Username = "";
            Password = "";
            LastIP = "";
            SelectedTexturePack = TexturePack.Default.Name;
            FavoriteServers = new FavoriteServer[0];
            IsFullscreen = false;
            InvertedMouse = false;
            // TODO: Why is the default resolution not contained in the static Defaults?
            WindowResolution = new WindowResolution(1280, 720);
        }

        public void Load()
        {
            if (File.Exists(Paths.Settings))
                JsonConvert.PopulateObject(File.ReadAllText(Paths.Settings), this);
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Paths.Settings));
            File.WriteAllText(Paths.Settings, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }

    public class FavoriteServer
    {
        public FavoriteServer(string name, string address)
        {
            Name = name;
            Address = address;
        }

        public string Name { get; }
        public string Address { get; }
    }

    public class WindowResolution
    {
        public static readonly WindowResolution[] Defaults =
            new WindowResolution[]
            {
                                                  // (from Wikipedia/other)
                new WindowResolution(800, 600),   // SVGA
                new WindowResolution(960, 640),   // DVGA
                new WindowResolution(1024, 600),  // WSVGA
                new WindowResolution(1024, 768),  // XGA
                new WindowResolution(1280, 1024), // SXGA
                new WindowResolution(1600, 1200), // UXGA
                new WindowResolution(1920, 1080), // big
                new WindowResolution(1920, 1200), // really big
                new WindowResolution(4096, 2160), // huge
            };

        public static WindowResolution FromString(string str)
        {
            string[] tmp = str.Split('x');
            int width = int.Parse(tmp[0].Trim());
            int height = int.Parse(tmp[1].Trim());
            return new WindowResolution(width, height);
        }

        public WindowResolution(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }
        public int Height { get; }

        public override string ToString()
        {
            return string.Format("{0} x {1}", Width, Height);
        }
    }
}