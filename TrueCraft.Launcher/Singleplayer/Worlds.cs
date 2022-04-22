using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using fNbt;
using TrueCraft.Core;
using TrueCraft.Core.Logic;
using TrueCraft.Core.World;
using TrueCraft.World;

namespace TrueCraft.Launcher.Singleplayer
{
    public class Worlds : IEnumerable<WorldInfo>
    {
        private readonly string _baseDirectory;

        private readonly List<WorldInfo> _savedWorlds;

        //public static Worlds Local { get; set; }

        // public IEnumerable<IWorld> Saves { get; }

        /// <summary>
        /// Constructs a new collection of WorldInfo objects.
        /// </summary>
        /// <param name="baseDirectory">The directory in which the World save folders are kept.</param>
        public Worlds(string baseDirectory)
        {
            if (!Directory.Exists(baseDirectory))
                throw new DirectoryNotFoundException(baseDirectory);

            _baseDirectory = baseDirectory;

            string[] directories = Directory.GetDirectories(baseDirectory);
            _savedWorlds = new List<WorldInfo>(directories.Length);
            foreach (string d in directories)
            {
                string manifestFile = Path.Combine(baseDirectory, "manifest.nbt");
                if (File.Exists(manifestFile))
                {
                    NbtFile file = new NbtFile(manifestFile);
                    _savedWorlds.Add(new WorldInfo(Path.GetFileName(d), file));
                }
            }
        }

        /// <summary>
        /// Gets the Base Directory where the Worlds in this list are stored.
        /// </summary>
        public string BaseDirectory { get => _baseDirectory; }

        /// <summary>
        /// Creates a new World.
        /// </summary>
        /// <param name="name">The World name, as entered by the Player</param>
        /// <param name="seed">The seed for generating the new World.</param>
        /// <returns></returns>
        public WorldInfo CreateNewWorld(string name, string seed)
        {
            int s;
            if (!int.TryParse(seed, out s))
            {
                // TODO: Hash seed string
                s = MathHelper.Random.Next();
            }

            string newWorldFolder = TrueCraft.World.World.CreateWorld(s, Paths.Worlds , name);
            string manifestFile = Path.Combine(newWorldFolder, "manifest.nbt");

            WorldInfo worldInfo = new WorldInfo(newWorldFolder, new NbtFile(manifestFile));
            _savedWorlds.Add(worldInfo);

            return worldInfo;
        }

        /// <summary>
        /// Removes the world in the specified directory from this collection
        /// </summary>
        /// <param name="directory"></param>
        public void Remove(string directory)
        {
            int j = 0;
            while (j < _savedWorlds.Count && _savedWorlds[j].Directory != directory)
                j++;
            if (j < _savedWorlds.Count)
                _savedWorlds.RemoveAt(j);
        }

        public IEnumerator<WorldInfo> GetEnumerator()
        {
            return _savedWorlds.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _savedWorlds.GetEnumerator();
        }
    }
}