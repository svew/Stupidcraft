using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using TrueCraft.API;
using TrueCraft.API.World;
using TrueCraft.API.Logic;
using fNbt;
using System.Collections;

namespace TrueCraft.Core.World
{
    public class World : IDisposable, IWorld, IEnumerable<IChunk>
    {
        public static readonly int Height = 128;

        public string Name { get; set; }
        private int _Seed;
        public int Seed
        {
            get { return _Seed; }
            set
            {
                _Seed = value;
                BiomeDiagram = new BiomeMap(_Seed);
            }
        }
        private Coordinates3D? _SpawnPoint;
        public Coordinates3D SpawnPoint
        {
            get
            {
                if (_SpawnPoint == null)
                    _SpawnPoint = ChunkProvider.GetSpawn(this);
                return _SpawnPoint.Value;
            }
            set
            {
                _SpawnPoint = value;
            }
        }
        public string BaseDirectory { get; internal set; }
        public IDictionary<Coordinates2D, IRegion> Regions { get; set; }
        public IBiomeMap BiomeDiagram { get; set; }
        public IChunkProvider ChunkProvider { get; set; }
        public IBlockRepository BlockRepository { get; set; }
        public DateTime BaseTime { get; set; }

        public long Time
        {
            get
            {
                return (long)((DateTime.UtcNow - BaseTime).TotalSeconds * 20) % 24000;
            }
            set
            {
                BaseTime = DateTime.UtcNow.AddSeconds(-value / 20);
            }
        }

        public event EventHandler<BlockChangeEventArgs> BlockChanged;
        public event EventHandler<ChunkLoadedEventArgs> ChunkGenerated;
        public event EventHandler<ChunkLoadedEventArgs> ChunkLoaded;

        public World()
        {
            Regions = new Dictionary<Coordinates2D, IRegion>();
            BaseTime = DateTime.UtcNow;
        }

        public World(string name) : this()
        {
            Name = name;
            Seed = MathHelper.Random.Next();
        }

        public World(string name, IChunkProvider chunkProvider) : this(name)
        {
            ChunkProvider = chunkProvider;
            ChunkProvider.Initialize(this);
        }

        public World(string name, int seed, IChunkProvider chunkProvider) : this(name, chunkProvider)
        {
            Seed = seed;
        }

        public static World LoadWorld(string baseDirectory)
        {
            if (!Directory.Exists(baseDirectory))
                throw new DirectoryNotFoundException();

            var world = new World(Path.GetFileName(baseDirectory));
            world.BaseDirectory = baseDirectory;

            if (File.Exists(Path.Combine(baseDirectory, "manifest.nbt")))
            {
                var file = new NbtFile(Path.Combine(baseDirectory, "manifest.nbt"));
                world.SpawnPoint = new Coordinates3D(file.RootTag["SpawnPoint"]["X"].IntValue,
                    file.RootTag["SpawnPoint"]["Y"].IntValue,
                    file.RootTag["SpawnPoint"]["Z"].IntValue);
                world.Seed = file.RootTag["Seed"].IntValue;
                var providerName = file.RootTag["ChunkProvider"].StringValue;
                var provider = (IChunkProvider)Activator.CreateInstance(Type.GetType(providerName));
                provider.Initialize(world);
                if (file.RootTag.Contains("Name"))
                    world.Name = file.RootTag["Name"].StringValue;
                world.ChunkProvider = provider;
            }

            return world;
        }

        /// <summary>
        /// Finds a chunk that contains the specified block coordinates.
        /// </summary>
        public IChunk FindChunk(Coordinates3D coordinates, bool generate = true)
        {
            IChunk chunk;
            FindBlockPosition(coordinates, out chunk, generate);
            return chunk;
        }

        public IChunk GetChunk(Coordinates2D coordinates, bool generate = true)
        {
            int regionX = coordinates.X / Region.Width - ((coordinates.X < 0) ? 1 : 0);
            int regionZ = coordinates.Z / Region.Depth - ((coordinates.Z < 0) ? 1 : 0);

            var region = LoadOrGenerateRegion(new Coordinates2D(regionX, regionZ), generate);
            if (region == null)
                return null;
            return region.GetChunk(new Coordinates2D(coordinates.X - regionX * 32, coordinates.Z - regionZ * 32), generate);
        }

        public void GenerateChunk(Coordinates2D coordinates)
        {
            int regionX = coordinates.X / Region.Width - ((coordinates.X < 0) ? 1 : 0);
            int regionZ = coordinates.Z / Region.Depth - ((coordinates.Z < 0) ? 1 : 0);

            var region = LoadOrGenerateRegion(new Coordinates2D(regionX, regionZ));
            region.GenerateChunk(new Coordinates2D(coordinates.X - regionX * 32, coordinates.Z - regionZ * 32));
        }

        public void SetChunk(Coordinates2D coordinates, Chunk chunk)
        {
            int regionX = coordinates.X / Region.Width - ((coordinates.X < 0) ? 1 : 0);
            int regionZ = coordinates.Z / Region.Depth - ((coordinates.Z < 0) ? 1 : 0);

            var region = LoadOrGenerateRegion(new Coordinates2D(regionX, regionZ));
            lock (region)
            {
                chunk.IsModified = true;
                region.SetChunk(new Coordinates2D(coordinates.X - regionX * 32, coordinates.Z - regionZ * 32), chunk);
            }
        }

        public void UnloadRegion(Coordinates2D coordinates)
        {
            lock (Regions)
            {
                Regions[coordinates].Save(Path.Combine(BaseDirectory, Region.GetRegionFileName(coordinates)));
                Regions.Remove(coordinates);
            }
        }

        public void UnloadChunk(Coordinates2D coordinates)
        {
            int regionX = coordinates.X / Region.Width - ((coordinates.X < 0) ? 1 : 0);
            int regionZ = coordinates.Z / Region.Depth - ((coordinates.Z < 0) ? 1 : 0);

            var regionPosition = new Coordinates2D(regionX, regionZ);
            if (!Regions.ContainsKey(regionPosition))
                throw new ArgumentOutOfRangeException("coordinates");
            Regions[regionPosition].UnloadChunk(new Coordinates2D(coordinates.X - regionX * 32, coordinates.Z - regionZ * 32));
        }

        public byte GetBlockID(Coordinates3D coordinates)
        {
            IChunk chunk;
            coordinates = FindBlockPosition(coordinates, out chunk);
            return chunk.GetBlockID(coordinates);
        }

        public byte GetMetadata(Coordinates3D coordinates)
        {
            IChunk chunk;
            coordinates = FindBlockPosition(coordinates, out chunk);
            return chunk.GetMetadata(coordinates);
        }

        public byte GetSkyLight(Coordinates3D coordinates)
        {
            IChunk chunk;
            coordinates = FindBlockPosition(coordinates, out chunk);
            return chunk.GetSkyLight(coordinates);
        }

        public byte GetBlockLight(Coordinates3D coordinates)
        {
            IChunk chunk;
            coordinates = FindBlockPosition(coordinates, out chunk);
            return chunk.GetBlockLight(coordinates);
        }

        public NbtCompound GetTileEntity(Coordinates3D coordinates)
        {
            IChunk chunk;
            coordinates = FindBlockPosition(coordinates, out chunk);
            return chunk.GetTileEntity(coordinates);
        }

        public BlockDescriptor GetBlockData(Coordinates3D coordinates)
        {
            IChunk chunk;
            var adjustedCoordinates = FindBlockPosition(coordinates, out chunk);
            return GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates);
        }

        public void SetBlockData(Coordinates3D coordinates, BlockDescriptor descriptor)
        {
            IChunk chunk;
            var adjustedCoordinates = FindBlockPosition(coordinates, out chunk);
            var old = GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates);
            chunk.SetBlockID(adjustedCoordinates, descriptor.ID);
            chunk.SetMetadata(adjustedCoordinates, descriptor.Metadata);
            if (BlockChanged != null)
                BlockChanged(this, new BlockChangeEventArgs(coordinates, old, GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates)));
        }

        private BlockDescriptor GetBlockDataFromChunk(Coordinates3D adjustedCoordinates, IChunk chunk, Coordinates3D coordinates)
        {
            return new BlockDescriptor
            {
                ID = chunk.GetBlockID(adjustedCoordinates),
                Metadata = chunk.GetMetadata(adjustedCoordinates),
                BlockLight = chunk.GetBlockLight(adjustedCoordinates),
                SkyLight = chunk.GetSkyLight(adjustedCoordinates),
                Coordinates = coordinates
            };
        }

        public void SetBlockID(Coordinates3D coordinates, byte value)
        {
            IChunk chunk;
            var adjustedCoordinates = FindBlockPosition(coordinates, out chunk);
            BlockDescriptor old = new BlockDescriptor();
            if (BlockChanged != null)
                old = GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates);
            chunk.SetBlockID(adjustedCoordinates, value);
            if (BlockChanged != null)
                BlockChanged(this, new BlockChangeEventArgs(coordinates, old, GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates)));
        }

        public void SetMetadata(Coordinates3D coordinates, byte value)
        {
            IChunk chunk;
            var adjustedCoordinates = FindBlockPosition(coordinates, out chunk);
            BlockDescriptor old = new BlockDescriptor();
            if (BlockChanged != null)
                old = GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates);
            chunk.SetMetadata(adjustedCoordinates, value);
            if (BlockChanged != null)
                BlockChanged(this, new BlockChangeEventArgs(coordinates, old, GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates)));
        }

        public void SetSkyLight(Coordinates3D coordinates, byte value)
        {
            IChunk chunk;
            var adjustedCoordinates = FindBlockPosition(coordinates, out chunk);
            BlockDescriptor old = new BlockDescriptor();
            if (BlockChanged != null)
                old = GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates);
            chunk.SetSkyLight(adjustedCoordinates, value);
            if (BlockChanged != null)
                BlockChanged(this, new BlockChangeEventArgs(coordinates, old, GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates)));
        }

        public void SetBlockLight(Coordinates3D coordinates, byte value)
        {
            IChunk chunk;
            var adjustedCoordinates = FindBlockPosition(coordinates, out chunk);
            BlockDescriptor old = new BlockDescriptor();
            if (BlockChanged != null)
                old = GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates);
            chunk.SetBlockLight(adjustedCoordinates, value);
            if (BlockChanged != null)
                BlockChanged(this, new BlockChangeEventArgs(coordinates, old, GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates)));
        }

        public void SetTileEntity(Coordinates3D coordinates, NbtCompound value)
        {
            IChunk chunk;
            coordinates = FindBlockPosition(coordinates, out chunk);
            chunk.SetTileEntity(coordinates, value);
        }

        public void Save()
        {
            lock (Regions)
            {
                foreach (var region in Regions)
                    region.Value.Save(Path.Combine(BaseDirectory, Region.GetRegionFileName(region.Key)));
            }
            var file = new NbtFile();
            file.RootTag.Add(new NbtCompound("SpawnPoint", new[]
            {
                new NbtInt("X", this.SpawnPoint.X),
                new NbtInt("Y", this.SpawnPoint.Y),
                new NbtInt("Z", this.SpawnPoint.Z)
            }));
            file.RootTag.Add(new NbtInt("Seed", this.Seed));
            file.RootTag.Add(new NbtString("ChunkProvider", this.ChunkProvider.GetType().FullName));
            file.RootTag.Add(new NbtString("Name", Name));
            file.SaveToFile(Path.Combine(this.BaseDirectory, "manifest.nbt"), NbtCompression.ZLib);
        }

        public void Save(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            BaseDirectory = path;
            Save();
        }

        private Dictionary<Thread, IChunk> ChunkCache = new Dictionary<Thread, IChunk>();
        private object ChunkCacheLock = new object();

        /// <summary>
        /// Finds the Coordinates of the Block within its Chunk.
        /// </summary>
        /// <param name="blockCoordinates">The Coordinates of the Block.</param>
        /// <param name="chunk">returns the Chunk containing the given Block Coordinates</param>
        /// <param name="generate">True to generate the Chunk, if it has not yet been generated.</param>
        /// <returns>The Local Block Coordinates within the Chunk.</returns>
        public Coordinates3D FindBlockPosition(Coordinates3D blockCoordinates, out IChunk chunk, bool generate = true)
        {
            if (blockCoordinates.Y < 0 || blockCoordinates.Y >= Chunk.Height)
                throw new ArgumentOutOfRangeException("coordinates", "Coordinates are out of range");

            Coordinates2D globalChunk = Coordinates.BlockToGlobalChunk(blockCoordinates);

            if (ChunkCache.ContainsKey(Thread.CurrentThread))
            {
                var cache = ChunkCache[Thread.CurrentThread];
                if (cache != null && globalChunk.X == cache.Coordinates.X && globalChunk.Z == cache.Coordinates.Z)
                    chunk = cache;
                else
                {
                    cache = GetChunk(globalChunk, generate);
                    lock (ChunkCacheLock)
                        ChunkCache[Thread.CurrentThread] = cache;
                }
            }
            else
            {
                var cache = GetChunk(globalChunk, generate);
                lock (ChunkCacheLock)
                    ChunkCache[Thread.CurrentThread] = cache;
            }

            chunk = GetChunk(globalChunk, generate);
            return Coordinates.GlobalBlockToLocalBlock(blockCoordinates);
        }

        public bool IsValidPosition(Coordinates3D position)
        {
            return position.Y >= 0 && position.Y < Chunk.Height;
        }

        /// <summary>
        /// Determines whether or not the Chunk containing the given Block Coordinates
        /// is loaded.
        /// </summary>
        /// <param name="blockCoordinates">The Block Coordinates to check.</param>
        /// <returns>True if the Chunk is loaded; false otherwise.</returns>
        public bool IsChunkLoaded(Coordinates3D blockCoordinates)
        {
            Coordinates2D regionCoordinates = Coordinates.BlockToRegion(blockCoordinates);
            if (!Regions.ContainsKey(regionCoordinates))
                return false;

            Coordinates2D local = Coordinates.BlockToLocalChunk(blockCoordinates);

            return Regions[regionCoordinates].Chunks.ContainsKey(local);
        }

        private Region LoadOrGenerateRegion(Coordinates2D coordinates, bool generate = true)
        {
            if (Regions.ContainsKey(coordinates))
                return (Region)Regions[coordinates];
            if (!generate)
                return null;
            Region region;
            if (BaseDirectory != null)
            {
                var file = Path.Combine(BaseDirectory, Region.GetRegionFileName(coordinates));
                if (File.Exists(file))
                    region = new Region(coordinates, this, file);
                else
                    region = new Region(coordinates, this);
            }
            else
                region = new Region(coordinates, this);
            lock (Regions)
                Regions[coordinates] = region;
            return region;
        }

        public void Dispose()
        {
            foreach (var region in Regions)
                region.Value.Dispose();
            BlockChanged = null;
            ChunkGenerated = null;
        }

        protected internal void OnChunkGenerated(ChunkLoadedEventArgs e)
        {
            if (ChunkGenerated != null)
                ChunkGenerated(this, e);
        }

        protected internal void OnChunkLoaded(ChunkLoadedEventArgs e)
        {
            if (ChunkLoaded != null)
                ChunkLoaded(this, e);
        }

        public class ChunkEnumerator : IEnumerator<IChunk>
        {
            public World World { get; set; }
            private int Index { get; set; }
            private IList<IChunk> Chunks { get; set; }

            public ChunkEnumerator(World world)
            {
                World = world;
                Index = -1;
                var regions = world.Regions.Values.ToList();
                var chunks = new List<IChunk>();
                foreach (var region in regions)
                    chunks.AddRange(region.Chunks.Values);
                Chunks = chunks;
            }

            public bool MoveNext()
            {
                Index++;
                return Index < Chunks.Count;
            }

            public void Reset()
            {
                Index = -1;
            }

            public void Dispose()
            {
            }

            public IChunk Current
            {
                get
                {
                    return Chunks[Index];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }
        }

        public IEnumerator<IChunk> GetEnumerator()
        {
            return new ChunkEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
