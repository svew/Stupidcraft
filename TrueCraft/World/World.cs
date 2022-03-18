using System;
using System.Collections.Generic;
using System.IO;
using fNbt;
using TrueCraft.Core;
using TrueCraft.Core.World;

namespace TrueCraft.World
{
    public class World : IWorld
    {
        private readonly List<IDimension> _dimensions;

        private readonly int _seed;

        private readonly string _baseDirectory;

        private readonly string _name;

        private PanDimensionalVoxelCoordinates _spawnPoint;

        public World(int seed, string name, List<IDimension> dimensions, PanDimensionalVoxelCoordinates spawnPoint)
        {
            _seed = seed;
            _name = name;
            _dimensions = dimensions;
            _spawnPoint = spawnPoint;
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

            IDimension overWorld = new Dimension(baseDirectory, "OverWorld");
            List<IDimension> dimensions = new List<IDimension>(1);
            dimensions.Add(null);   // TODO space reserved for Nether
            dimensions.Add(overWorld);

            return new World(seed, name, dimensions, spawnPoint);
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
            file.RootTag.Add(new NbtString("ChunkProvider", this[DimensionID.Overworld].ChunkProvider.GetType().FullName));
            file.RootTag.Add(new NbtString("Name", Name));
            file.SaveToFile(Path.Combine(this._baseDirectory, "manifest.nbt"), NbtCompression.ZLib);

            foreach (IDimension dimension in _dimensions)
                dimension.Save();
        }
        #endregion
    }
}
