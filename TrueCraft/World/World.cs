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

        /// <summary>
        /// The full path to the folder where the world is saved.
        /// </summary>
        /// <remarks>The last element of this path is a sanitized (for filesystem)
        /// version of _name.</remarks>
        private readonly string _baseDirectory;

        /// <summary>
        /// The name of the World, as seen by the Player
        /// </summary>
        private readonly string _name;

        private PanDimensionalVoxelCoordinates _spawnPoint;

        /// <summary>
        /// Constructs a new World instance
        /// </summary>
        /// <param name="serviceLocator"></param>
        /// <param name="seed">The seed to be used to generate the World.</param>
        /// <param name="baseDirectory">The folder this world is saved.</param>
        /// <param name="name">The name of the World, as seen by the Player.</param>
        /// <param name="dimensionFactory">A Factory for building the set of Dimensions.</param>
        /// <param name="spawnPoint">The default Spawn Point for all Players.</param>
        private World(IServiceLocator serviceLocator, int seed, string baseDirectory,
            string name, IDimensionFactory dimensionFactory, PanDimensionalVoxelCoordinates spawnPoint)
        {
            _seed = seed;
            _name = name;
            _baseDirectory = baseDirectory;

            IList<IDimensionServer> dimensions = dimensionFactory.BuildDimensions(serviceLocator, baseDirectory, seed);
            _dimensions = new List<IDimensionServer>(dimensions.Count);
            _dimensions.AddRange(dimensions);

            _spawnPoint = spawnPoint;
        }

        /// <summary>
        /// Creates a new World
        /// </summary>
        /// <param name="seed">The seed to be used to generate the World.</param>
        /// <param name="baseDirectory">The folder where worlds are saved.
        /// This will be the parent folder of the folder where the new world is created.</param>
        /// <param name="name">The name of the World, as seen by the Player.</param>
        /// <returns>The full path to the folder containing the manifest.nbt file.  The last
        /// element of this path is a sanitized (for illegal file system characters and uniqueness)
        /// version of name.</returns>
        public static string CreateWorld(int seed, string baseDirectory, string name)
        {
            // Ensure that the folder name does not contain any illegal characters.
            string safeName = name;
            foreach (char c in Path.GetInvalidFileNameChars())
                safeName = safeName.Replace(c.ToString(), string.Empty);

            // Ensure that the folder name does not duplicate an existing folder name
            if (File.Exists(Path.Combine(baseDirectory, safeName)))
            {
                int serial = 1;
                while (File.Exists(Path.Combine(baseDirectory, $"{safeName}{serial}")))
                    serial++;
                safeName = $"{safeName}{serial}";
            }

            // Create the folder
            string worldFolder = Path.Combine(baseDirectory, safeName);
            Directory.CreateDirectory(worldFolder);

            NbtFile file = new NbtFile();
            file.RootTag.Add(new NbtCompound("SpawnPoint", new[]
            {
                new NbtInt("X", 0),
                new NbtInt("Y", 0),
                new NbtInt("Z", 0)
            }));
            file.RootTag.Add(new NbtInt("Seed", seed));
            // TODO fix hard-coded OverWorld chunk provider
            file.RootTag.Add(new NbtString("ChunkProvider", "TrueCraft.TerrainGen.StandardGenerator"));
            file.RootTag.Add(new NbtString("Name", name));
            file.SaveToFile(Path.Combine(worldFolder, "manifest.nbt"), NbtCompression.ZLib);

            return worldFolder;
        }

        public static IWorld LoadWorld(IServiceLocator serviceLocator, string baseDirectory)
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
                Type? chunkProviderType = Type.GetType(providerName);
                if (chunkProviderType is null)
                    throw new MissingProviderException(providerName);
                IChunkProvider provider = (IChunkProvider)Activator.CreateInstance(chunkProviderType,
                          new object[] { seed })!;
                // TODO
                // provider.Initialize(dimension);

                if (file.RootTag.Contains("Name"))
                    name = file.RootTag["Name"].StringValue;

                // TODO
                // dimension.ChunkProvider = provider;
            }

            IDimensionFactory factory = new DimensionFactory();
            return new World(serviceLocator, seed, baseDirectory, name, factory, spawnPoint);
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
        public string BaseDirectory { get => _baseDirectory; }

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
