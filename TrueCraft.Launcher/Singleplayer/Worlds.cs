using System;
using System.Collections.Generic;
using System.IO;
using TrueCraft.Core;
using TrueCraft.Core.Logic;
using TrueCraft.Core.World;
using TrueCraft.World;

namespace TrueCraft.Launcher.Singleplayer
{
    public class Worlds
    {
        private List<IWorld> _savedWorlds;

        public static Worlds Local { get; set; }

        public IEnumerable<IWorld> Saves { get; }

        public void Load()
        {
            if (!Directory.Exists(Paths.Worlds))
                Directory.CreateDirectory(Paths.Worlds);

            string[] directories = Directory.GetDirectories(Paths.Worlds);
            _savedWorlds = new List<IWorld>(directories.Length);
            foreach (string d in directories)
            {
                try
                {
                    // TODO: Instead of "loading" every world, have a method to check for validity.
                    IWorld w = TrueCraft.World.World.LoadWorld(d);
                    _savedWorlds.Add(w);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        /// <summary>
        /// Creates a new World.
        /// </summary>
        /// <param name="name">The World name, as entered by the Player</param>
        /// <param name="seed">The seed for generating the new World.</param>
        /// <returns></returns>
        public IWorld CreateNewWorld(string name, string seed)
        {
            int s;
            if (!int.TryParse(seed, out s))
            {
                // TODO: Hash seed string
                s = MathHelper.Random.Next();
            }

            Discover.DoDiscovery(new Discover());

            PanDimensionalVoxelCoordinates spawnPoint = new PanDimensionalVoxelCoordinates(DimensionID.Overworld, 0, 0, 0);
            IDimensionFactory factory = new DimensionFactory();
            IWorld world = new TrueCraft.World.World(s, Paths.Worlds, name, factory, spawnPoint);
            world.Save();
            _savedWorlds.Add(world);

            return world;
        }
    }
}