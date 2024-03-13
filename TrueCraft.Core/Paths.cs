using System;
using System.IO;

namespace TrueCraft.Core
{
    public static class Paths
    {
        public static string Base { get; private set; } = FindBasePath();

        public static string Worlds => Path.Combine(Base, "worlds");
        public static string Settings => Path.Combine(Base, "settings.json");
        public static string Screenshots => Path.Combine(Base, "screenshots");
        public static string TexturePacks => Path.Combine(Base, "texturepacks");

        private static string FindBasePath()
        {
            string basePath;

            string? xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            if (xdgConfigHome is not null)
            {
                basePath = Path.Combine(xdgConfigHome, "truecraft");
                if (Directory.Exists(basePath))
                {
                    return basePath;
                }
            }

            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            basePath = Path.Combine(appDataFolder, "truecraft");
            if (Directory.Exists(basePath))
            {
                return basePath;
            }

            var userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            basePath = Path.Combine(userprofile, ".truecraft");
            if (Directory.Exists(userprofile))
            {
                return userprofile;
            }

            // At this point, there's no existing data to choose from, so use the best option
            if (xdgConfigHome is not null)
            {
                basePath = Path.Combine(xdgConfigHome, "truecraft");
                Directory.CreateDirectory(basePath);
                return basePath;
            }
            else
            {
                basePath = Path.Combine(appDataFolder, "truecraft");
                Directory.CreateDirectory(basePath);
                return basePath;
            }
        }
    }
}
