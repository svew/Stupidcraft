using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using fNbt;
using TrueCraft.Core.Logic;

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
        private GlobalVoxelCoordinates _SpawnPoint = null;

        public GlobalVoxelCoordinates SpawnPoint
        {
            get
            {
                if (object.ReferenceEquals(_SpawnPoint, null))
                    _SpawnPoint = ChunkProvider.GetSpawn(this);
                return _SpawnPoint;
            }
            set
            {
                _SpawnPoint = value;
            }
        }
        public string BaseDirectory { get; internal set; }

        private IDictionary<RegionCoordinates, IRegion> _regions;

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
            _regions = new Dictionary<RegionCoordinates, IRegion>();
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
                world.SpawnPoint = new GlobalVoxelCoordinates(file.RootTag["SpawnPoint"]["X"].IntValue,
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
        public IChunk FindChunk(GlobalVoxelCoordinates coordinates, bool generate = true)
        {
            IChunk chunk;
            FindBlockPosition(coordinates, out chunk, generate);
            return chunk;
        }

        /// <summary>
        /// Gets the specified Chunk 
        /// </summary>
        /// <param name="chunkCoords">The Global Chunk Coordinates to retrieve</param>
        /// <param name="generate">True to generate the Chunk if it has never been generated.</param>
        /// <returns>A reference to the Chunk.  This may return null.</returns>
        public IChunk GetChunk(GlobalChunkCoordinates chunkCoords, bool generate = true)
        {
            RegionCoordinates regionCoords = (RegionCoordinates)chunkCoords;

            var region = LoadOrGenerateRegion(regionCoords, generate);
            if (region == null)
                return null;

            return region.GetChunk((LocalChunkCoordinates)chunkCoords, generate);
        }

        /// <summary>
        /// Sets the specified Chunk in the World.
        /// </summary>
        /// <param name="globalChunk">The Global Chunk Coordinates of the Chunk.</param>
        /// <param name="chunk">The Chunk to add to the world.</param>
        public void SetChunk(GlobalChunkCoordinates globalChunk, Chunk chunk)
        {
            RegionCoordinates regionCoords = (RegionCoordinates)globalChunk;
            LocalChunkCoordinates localChunk = (LocalChunkCoordinates)globalChunk;

            var region = LoadOrGenerateRegion(regionCoords);
            lock (region)
            {
                chunk.IsModified = true;
                region.SetChunk(localChunk, chunk);
            }
        }

        public void UnloadRegion(RegionCoordinates coordinates)
        {
            lock (_regions)
            {
                _regions[coordinates].Save(Path.Combine(BaseDirectory, Region.GetRegionFileName(coordinates)));
                _regions.Remove(coordinates);
            }
        }

        /// <summary>
        /// Unloads the specified Chunk
        /// </summary>
        /// <param name="globalChunk">The Global Chunk Coordinates of the Chunk to unload.</param>
        public void UnloadChunk(GlobalChunkCoordinates globalChunk)
        {
            RegionCoordinates regionCoords = (RegionCoordinates)globalChunk;
            LocalChunkCoordinates localCoords = (LocalChunkCoordinates)globalChunk;

            if (!_regions.ContainsKey(regionCoords))
                throw new ArgumentOutOfRangeException("coordinates");

            _regions[regionCoords].UnloadChunk(localCoords);
        }

        public byte GetBlockID(GlobalVoxelCoordinates coordinates)
        {
            IChunk chunk;
            LocalVoxelCoordinates local = FindBlockPosition(coordinates, out chunk);
            return chunk.GetBlockID(local);
        }

        public byte GetMetadata(GlobalVoxelCoordinates coordinates)
        {
            IChunk chunk;
            LocalVoxelCoordinates local = FindBlockPosition(coordinates, out chunk);
            return chunk.GetMetadata(local);
        }

        public byte GetSkyLight(GlobalVoxelCoordinates coordinates)
        {
            IChunk chunk;
            LocalVoxelCoordinates local = FindBlockPosition(coordinates, out chunk);
            return chunk.GetSkyLight(local);
        }

        public byte GetBlockLight(GlobalVoxelCoordinates coordinates)
        {
            IChunk chunk;
            LocalVoxelCoordinates local = FindBlockPosition(coordinates, out chunk);
            return chunk.GetBlockLight(local);
        }

        public NbtCompound GetTileEntity(GlobalVoxelCoordinates coordinates)
        {
            IChunk chunk;
            LocalVoxelCoordinates local = FindBlockPosition(coordinates, out chunk);
            return chunk.GetTileEntity(local);
        }

        public BlockDescriptor GetBlockData(GlobalVoxelCoordinates coordinates)
        {
            IChunk chunk;
            LocalVoxelCoordinates local = FindBlockPosition(coordinates, out chunk);
            return GetBlockDataFromChunk(local, chunk, coordinates);
        }

        public void SetBlockData(GlobalVoxelCoordinates coordinates, BlockDescriptor descriptor)
        {
            IChunk chunk;
            var adjustedCoordinates = FindBlockPosition(coordinates, out chunk);
            var old = GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates);
            chunk.SetBlockID(adjustedCoordinates, descriptor.ID);
            chunk.SetMetadata(adjustedCoordinates, descriptor.Metadata);
            if (BlockChanged != null)
                BlockChanged(this, new BlockChangeEventArgs(coordinates, old, GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates)));
        }

        private BlockDescriptor GetBlockDataFromChunk(LocalVoxelCoordinates adjustedCoordinates, IChunk chunk, GlobalVoxelCoordinates coordinates)
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

        public void SetBlockID(GlobalVoxelCoordinates coordinates, byte value)
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

        public void SetMetadata(GlobalVoxelCoordinates coordinates, byte value)
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

        public void SetSkyLight(GlobalVoxelCoordinates coordinates, byte value)
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

        public void SetBlockLight(GlobalVoxelCoordinates coordinates, byte value)
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

        public void SetTileEntity(GlobalVoxelCoordinates coordinates, NbtCompound value)
        {
            IChunk chunk;
            LocalVoxelCoordinates local = FindBlockPosition(coordinates, out chunk);
            chunk.SetTileEntity(local, value);
        }

        public void Save()
        {
            lock (_regions)
            {
                foreach (var region in _regions)
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
        public LocalVoxelCoordinates FindBlockPosition(GlobalVoxelCoordinates blockCoordinates, out IChunk chunk, bool generate = true)
        {
            if (blockCoordinates.Y < 0 || blockCoordinates.Y >= Chunk.Height)
                throw new ArgumentOutOfRangeException("coordinates", "Coordinates are out of range");

            GlobalChunkCoordinates globalChunk = (GlobalChunkCoordinates)blockCoordinates;

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
            return (LocalVoxelCoordinates)blockCoordinates;
        }

        public bool IsValidPosition(GlobalVoxelCoordinates position)
        {
            return position.Y >= 0 && position.Y < Chunk.Height;
        }

        /// <summary>
        /// Determines whether or not the Chunk containing the given Block Coordinates
        /// is loaded.
        /// </summary>
        /// <param name="blockCoordinates">The Block Coordinates to check.</param>
        /// <returns>True if the Chunk is loaded; false otherwise.</returns>
        public bool IsChunkLoaded(GlobalVoxelCoordinates blockCoordinates)
        {
            RegionCoordinates regionCoordinates = (RegionCoordinates)blockCoordinates;
            if (!_regions.ContainsKey(regionCoordinates))
                return false;

            LocalChunkCoordinates local = (LocalChunkCoordinates)blockCoordinates;

            return _regions[regionCoordinates].IsChunkLoaded(local);
        }

        private Region LoadOrGenerateRegion(RegionCoordinates coordinates, bool generate = true)
        {
            if (_regions.ContainsKey(coordinates))
                return (Region)_regions[coordinates];
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
            lock (_regions)
                _regions[coordinates] = region;
            return region;
        }

        public void Dispose()
        {
            foreach (var region in _regions)
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
                var regions = world._regions.Values.ToList();
                var chunks = new List<IChunk>();
                foreach (var region in regions)
                    chunks.AddRange(region.Chunks);
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
