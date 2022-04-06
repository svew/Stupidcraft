using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using fNbt;
using TrueCraft.Core;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.World
{
    public class World : IWorld
    {
        private readonly List<IDimensionServer> _dimensions;

        private readonly int _seed;

        private readonly string _baseDirectory;

        /// <summary>
        /// The name of the World, as seen by the Player
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The full path to the folder where the world is saved.
        /// </summary>
        /// <remarks>The last element of this path is a sanitized (for filesystem)
        /// version of _name.</remarks>
        private readonly string _folderName;

        private PanDimensionalVoxelCoordinates _spawnPoint;

        /// <summary>
        /// Creates a new World
        /// </summary>
        /// <param name="seed">The seed to be used to generate the World.</param>
        /// <param name="baseDirectory">The folder where worlds are saved.</param>
        /// <param name="name">The name of the World, as seen by the Player.</param>
        /// <param name="dimensionFactory">A Factory for building the set of Dimensions.</param>
        /// <param name="spawnPoint">The default Spawn Point for all Players.</param>
        public World(int seed, string baseDirectory, string name, IDimensionFactory dimensionFactory, PanDimensionalVoxelCoordinates spawnPoint)
        {
            _seed = seed;
            _name = name;
            _baseDirectory = baseDirectory;

            IList<IDimensionServer> dimensions = dimensionFactory.BuildDimensions(baseDirectory, seed);
            _dimensions = new List<IDimensionServer>(dimensions.Count);
            _dimensions.AddRange(dimensions);

            _spawnPoint = spawnPoint;

            string safeName = name;
            foreach (char c in Path.GetInvalidFileNameChars())
                safeName = safeName.Replace(c.ToString(), string.Empty);
            _folderName = Path.Combine(baseDirectory, safeName);
        }

        public static IWorld LoadWorld(string baseDirectory)
        {
            if (!Directory.Exists(baseDirectory))
                throw new DirectoryNotFoundException();

            string name = Path.GetFileName(baseDirectory);
            PanDimensionalVoxelCoordinates spawnPoint = new PanDimensionalVoxelCoordinates(DimensionID.Overworld, 0, 0, 0);
            int seed = 0;

            if (File.Exists(Path.Combine(baseDirectory, "manifest.nbt")))
            {
                NbtFile file = new NbtFile(Path.Combine(baseDirectory, "manifest.nbt"));

                NbtCompound spawnNbt = (NbtCompound)file.RootTag["SpawnPoint"];
                int x = spawnNbt["X"].IntValue;
                int y = spawnNbt["Y"].IntValue;
                int z = spawnNbt["Z"].IntValue;
                spawnPoint = new PanDimensionalVoxelCoordinates(DimensionID.Overworld, x, y, z);

                seed = file.RootTag["Seed"].IntValue;

                string providerName = file.RootTag["ChunkProvider"].StringValue;
                IChunkProvider provider = (IChunkProvider)Activator.CreateInstance(Type.GetType(providerName));
                // TODO
                // provider.Initialize(dimension);

                if (file.RootTag.Contains("Name"))
                    name = file.RootTag["Name"].StringValue;

                // TODO
                // dimension.ChunkProvider = provider;
            }

            IDimensionFactory factory = new DimensionFactory();
            return new World(seed, baseDirectory, name, factory, spawnPoint);
        }

        #region IWorld
        /// <inheritdoc />
        public IDimension this[DimensionID index]
        {
            get
            {
                return _dimensions[(int)index];
            }
        }

        /// <inheritdoc />
        public int Seed { get => _seed; }

        /// <inheritdoc />
        public string Name { get => _name; }

        /// <inheritdoc />
        public PanDimensionalVoxelCoordinates SpawnPoint { get => _spawnPoint; }

        /// <inheritdoc />
        public int Count { get => _dimensions.Count; }

        /// <inheritdoc />
        public void Save()
        {
            NbtFile file = new NbtFile();
            file.RootTag.Add(new NbtCompound("SpawnPoint", new[]
            {
                new NbtInt("X", this.SpawnPoint.X),
                new NbtInt("Y", this.SpawnPoint.Y),
                new NbtInt("Z", this.SpawnPoint.Z)
            }));
            file.RootTag.Add(new NbtInt("Seed", this.Seed));
            file.RootTag.Add(new NbtString("ChunkProvider", ((IDimensionServer)this[DimensionID.Overworld]).ChunkProvider));
            file.RootTag.Add(new NbtString("Name", Name));
            file.SaveToFile(Path.Combine(this._baseDirectory, "manifest.nbt"), NbtCompression.ZLib);

            foreach (IDimensionServer dimension in _dimensions)
                dimension.Save();
        }

        public IEnumerator<IDimensionServer> GetEnumerator()
        {
            return _dimensions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dimensions.GetEnumerator();
        }
        #endregion
    }
}
