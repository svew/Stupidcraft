using System;
using System.IO;
using System.IO.Compression;

namespace TrueCraft.Core
{
    /// <summary>
    /// Represents a Minecraft 1.7.3 texture pack (.zip archive).
    /// </summary>
    public class TexturePack
    {
        public static readonly TexturePack Unknown = new TexturePack(
            "?",
            File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content/default-pack.png")),
            File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content/default-pack.txt")));

        public static readonly TexturePack Default = new TexturePack(
            "Default",
            File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content/pack.png")),
            File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content/pack.txt")));

        public static TexturePack? FromArchive(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException();

            string description = Unknown.Description;
            Stream image = Unknown.Image;

            using (Stream strm = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (ZipArchive archive = new ZipArchive(strm))
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Name == "pack.txt")
                    {
                        using (Stream stream = entry.Open())
                        using (StreamReader reader = new StreamReader(stream))
                            description = reader.ReadToEnd().TrimEnd('\n', '\r', ' ');
                    }
                    else if (entry.Name == "pack.png")
                    {
                        using (Stream stream = entry.Open())
                        {
                            byte[] buffer = new byte[entry.Length];
                            image = new MemoryStream((int)entry.Length);
                            int nBytes;
                            do
                            {
                                nBytes = stream.Read(buffer, 0, buffer.Length);
                                if (nBytes > 0)
                                    image.Write(buffer, 0, nBytes);
                            } while (nBytes > 0);

                            // Fixes 'GLib.GException: Unrecognized image file format' on Linux.
                            image.Seek(0, SeekOrigin.Begin);
                        }
                    }
                }

            string name = new FileInfo(path).Name;
            return new TexturePack(name, image, description);
        }

        public string Name { get; private set; }

        public Stream Image { get; private set; }

        public string Description { get; private set; }

        public TexturePack(string name, Stream image, string description)
        {
            Name = name;
            Image = image;
            Description = description;
        }
    }
}
