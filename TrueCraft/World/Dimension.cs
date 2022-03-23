using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using fNbt;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.World;

namespace TrueCraft.World
{
    public class Dimension : IDisposable, IDimension, IEnumerable<IChunk>, IEquatable<IDimension>
    {
        public static readonly int Height = 128;

        private readonly DimensionID _dimensionID;

        /// <inheritdoc />
        public string Name { get; }

        public readonly string _baseDirectory;

        private readonly IDictionary<RegionCoordinates, IRegion> _regions;

        private readonly IChunkProvider _chunkProvider;

        private readonly IBlockRepository _blockRepository;

        /// <inheritdoc/>
        public IBlockRepository BlockRepository { get => _blockRepository; }

        private DateTime _baseTime;

        public long TimeOfDay
        {
            get
            {
                return (long)((DateTime.UtcNow - _baseTime).TotalSeconds * 20) % 24000;
            }
            set
            {
                _baseTime = DateTime.UtcNow.AddSeconds(-value / 20);
            }
        }

        public event EventHandler<BlockChangeEventArgs>? BlockChanged;
        public event EventHandler<ChunkLoadedEventArgs>? ChunkGenerated;
        public event EventHandler<ChunkLoadedEventArgs>? ChunkLoaded;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseDirectory"></param>
        /// <param name="dimensionID"></param>
        /// <param name="chunkProvider"></param>
        /// <param name="blockRepository"></param>
        public Dimension(string baseDirectory, DimensionID dimensionID, IChunkProvider chunkProvider, IBlockRepository blockRepository)
        {
            _dimensionID = dimensionID; ;
            _baseDirectory = baseDirectory;
            Name = DimensionInfo.GetName(dimensionID);
            _chunkProvider = chunkProvider;
            _blockRepository = blockRepository;
            _regions = new Dictionary<RegionCoordinates, IRegion>();
            _baseTime = DateTime.UtcNow;
        }

        private static string DimensionIDToFolder(DimensionID id)
        {
            if (id == DimensionID.Overworld)
                return string.Empty;

            return "DIM" + (byte)id;
        }

        #region object overrides
        public override int GetHashCode()
        {
            return _dimensionID.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as IDimension);
        }

        public override string ToString()
        {
            return Name;
        }
        #endregion

        #region IEquatable<IDimension>
        public bool Equals(IDimension? other)
        {
            if (other is null)
                return false;

            return _dimensionID == other.ID;
        }
        #endregion

        /// <inheritdoc />
        public DimensionID ID { get => _dimensionID; }

        /// <inheritdoc />
        public IChunk? GetChunk(GlobalVoxelCoordinates coordinates)
        {
            IRegion region = GetRegion((RegionCoordinates)coordinates);
            return region.GetChunk((LocalChunkCoordinates)coordinates);
        }

        /// <inheritdoc />
        public IChunk? GetChunk(GlobalChunkCoordinates chunkCoords)
        {
            RegionCoordinates regionCoords = (RegionCoordinates)chunkCoords;

            IRegion region = GetRegion(regionCoords);

            return region.GetChunk((LocalChunkCoordinates)chunkCoords);
        }

        /// <inheritdoc />
        public byte GetBlockID(GlobalVoxelCoordinates coordinates)
        {
            IChunk? chunk;
            LocalVoxelCoordinates local = FindBlockPosition(coordinates, out chunk);
            return chunk?.GetBlockID(local) ?? AirBlock.BlockID;
        }

        /// <inheritdoc />
        public byte GetMetadata(GlobalVoxelCoordinates coordinates)
        {
            IChunk? chunk;
            LocalVoxelCoordinates local = FindBlockPosition(coordinates, out chunk);
            return chunk?.GetMetadata(local) ?? 0;
        }

        /// <inheritdoc />
        public byte GetSkyLight(GlobalVoxelCoordinates coordinates)
        {
            IChunk? chunk;
            LocalVoxelCoordinates local = FindBlockPosition(coordinates, out chunk);
            return chunk?.GetSkyLight(local) ?? 0;
        }

        /// <inheritdoc />
        public byte GetBlockLight(GlobalVoxelCoordinates coordinates)
        {
            IChunk? chunk;
            LocalVoxelCoordinates local = FindBlockPosition(coordinates, out chunk);
            return chunk?.GetBlockLight(local) ?? 0;
        }

        /// <inheritdoc />
        public NbtCompound? GetTileEntity(GlobalVoxelCoordinates coordinates)
        {
            IChunk? chunk;
            LocalVoxelCoordinates local = FindBlockPosition(coordinates, out chunk);
            return chunk?.GetTileEntity(local) ?? null;
        }

        /// <inheritdoc />
        public BlockDescriptor GetBlockData(GlobalVoxelCoordinates coordinates)
        {
            IChunk? chunk;
            LocalVoxelCoordinates local = FindBlockPosition(coordinates, out chunk);
            if (chunk is null)
            {
                BlockDescriptor rv = new BlockDescriptor();
                rv.ID = AirBlock.BlockID;
                rv.Metadata = 0;
                rv.BlockLight = 0;
                rv.SkyLight = 0;
                rv.Coordinates = coordinates;
                return rv;
            }

            return GetBlockDataFromChunk(local, chunk, coordinates);
        }

        /// <inheritdoc />
        public void SetBlockData(GlobalVoxelCoordinates coordinates, BlockDescriptor descriptor)
        {
            IChunk? chunk;
            LocalVoxelCoordinates adjustedCoordinates = FindBlockPosition(coordinates, out chunk);
            if (chunk is null)
                return;

            BlockDescriptor old = GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates);

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

        /// <inheritdoc />
        public void SetBlockID(GlobalVoxelCoordinates coordinates, byte value)
        {
            IChunk? chunk;
            LocalVoxelCoordinates adjustedCoordinates = FindBlockPosition(coordinates, out chunk);
            if (chunk is null)
                return;

            BlockDescriptor old = new BlockDescriptor();
            if (BlockChanged != null)
                old = GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates);

            chunk.SetBlockID(adjustedCoordinates, value);

            if (BlockChanged != null)
                BlockChanged(this, new BlockChangeEventArgs(coordinates, old, GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates)));
        }

        /// <inheritdoc />
        public void SetMetadata(GlobalVoxelCoordinates coordinates, byte value)
        {
            IChunk? chunk;
            var adjustedCoordinates = FindBlockPosition(coordinates, out chunk);
            if (chunk is null)
                return;

            BlockDescriptor old = new BlockDescriptor();
            if (BlockChanged != null)
                old = GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates);

            chunk.SetMetadata(adjustedCoordinates, value);

            if (BlockChanged != null)
                BlockChanged(this, new BlockChangeEventArgs(coordinates, old, GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates)));
        }

        /// <inheritdoc />
        public void SetSkyLight(GlobalVoxelCoordinates coordinates, byte value)
        {
            IChunk? chunk;
            LocalVoxelCoordinates adjustedCoordinates = FindBlockPosition(coordinates, out chunk);
            if (chunk is null)
                return;

            BlockDescriptor old = new BlockDescriptor();
            if (BlockChanged != null)
                old = GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates);

            chunk.SetSkyLight(adjustedCoordinates, value);

            if (BlockChanged != null)
                BlockChanged(this, new BlockChangeEventArgs(coordinates, old, GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates)));
        }

        /// <inheritdoc />
        public void SetBlockLight(GlobalVoxelCoordinates coordinates, byte value)
        {
            IChunk? chunk;
            LocalVoxelCoordinates adjustedCoordinates = FindBlockPosition(coordinates, out chunk);
            if (chunk is null)
                return;

            BlockDescriptor old = new BlockDescriptor();
            if (BlockChanged != null)
                old = GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates);

            chunk.SetBlockLight(adjustedCoordinates, value);

            if (BlockChanged != null)
                BlockChanged(this, new BlockChangeEventArgs(coordinates, old, GetBlockDataFromChunk(adjustedCoordinates, chunk, coordinates)));
        }

        /// <inheritdoc />
        public void SetTileEntity(GlobalVoxelCoordinates coordinates, NbtCompound value)
        {
            IChunk? chunk;
            LocalVoxelCoordinates local = FindBlockPosition(coordinates, out chunk);
            if (chunk is null)
                return;
            chunk.SetTileEntity(local, value);
        }

        /// <inheritdoc />
        public void Save()
        {
            lock (_regions)
            {
                foreach (var region in _regions)
                    region.Value.Save(Path.Combine(_baseDirectory, Region.GetRegionFileName(region.Key)));
            }
        }

        /// <inheritdoc />
        public LocalVoxelCoordinates FindBlockPosition(GlobalVoxelCoordinates blockCoordinates, out IChunk? chunk)
        {
            GlobalChunkCoordinates globalChunk = (GlobalChunkCoordinates)blockCoordinates;
            chunk = GetChunk(globalChunk);

            return (LocalVoxelCoordinates)blockCoordinates;
        }

        /// <inheritdoc />
        public bool IsValidPosition(GlobalVoxelCoordinates position)
        {
            return position.Y >= 0 && position.Y < Chunk.Height;
        }

        /// <inheritdoc />
        public bool IsChunkLoaded(GlobalVoxelCoordinates blockCoordinates)
        {
            RegionCoordinates regionCoordinates = (RegionCoordinates)blockCoordinates;
            if (!_regions.ContainsKey(regionCoordinates))
                return false;

            LocalChunkCoordinates local = (LocalChunkCoordinates)blockCoordinates;

            return _regions[regionCoordinates].IsChunkLoaded(local);
        }

        private IRegion GetRegion(RegionCoordinates coordinates)
        {
            lock(_regions)
            {
                if (_regions.ContainsKey(coordinates))
                    return (Region)_regions[coordinates];
            }

            IRegion region = new Region(coordinates, _baseDirectory);
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
            ChunkLoaded = null;
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

        public IEnumerator<IChunk> GetEnumerator()
        {
            List<IChunk> chunks = new List<IChunk>();
            foreach (IRegion region in _regions.Values)
                foreach (IChunk chunk in region.Chunks)
                    chunks.Add(chunk);

            return chunks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
