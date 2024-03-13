using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using fNbt;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;

namespace TrueCraft.World
{
    /// <summary>
    /// Represents a 32x32 area of <see cref="Chunk"/> objects.
    /// Not all of these chunks are in memory at any given time.
    /// </summary>
    internal class Region : IDisposable, IRegion
    {
        /// <summary>
        /// The number of chunks within the region in the X-direction.
        /// </summary>
        public const int Width = WorldConstants.RegionWidth;

        /// <summary>
        /// The number of chunks within the region in the Z-direction.
        /// </summary>
        public const int Depth = WorldConstants.RegionDepth;

        private readonly IChunk[,] _chunks = new IChunk[Width, Depth];

        /// <summary>
        /// The location of this region in the overworld.
        /// </summary>
        public RegionCoordinates Position { get; }

        private HashSet<LocalChunkCoordinates> DirtyChunks { get; } = new(Width * Depth);

        private Stream _regionFileStream;
        private object _streamLock = new();

        public event EventHandler<ChunkLoadedEventArgs>? ChunkLoaded;

        /// <summary>
        /// Creates a new Region for server-side use at the given position in
        /// the given World.
        /// </summary>
        /// <params>
        /// <param name="position">The Position of the Region within the parent Dimension</param>
        /// <param name="baseDirectory">The folder in which the parent Dimension is stored.</param>
        /// </params>
        public Region(RegionCoordinates position, string baseDirectory)
        {
            Position = position;

            string regionFolder = Path.Combine(baseDirectory, "region");
            if (!Directory.Exists(regionFolder))
            {
                Directory.CreateDirectory(regionFolder);
            }
            string regionFile = Path.Combine(regionFolder, GetRegionFileName(position));
            if (File.Exists(regionFile))
            {
                _regionFileStream = File.Open(regionFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                _regionFileStream.Read(HeaderCache, 0, 8192);
            }
            else
            {
                _regionFileStream = File.Open(regionFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                CreateRegionHeader();
            }
        }

        public IEnumerable<IChunk> Chunks
        {
            get
            {
                List<IChunk> lst = new List<IChunk>();
                for (int j = 0; j < Width; j++)
                    for (int k = 0; k < Depth; k++)
                    {
                        IChunk? chunk = _chunks[j, k];
                        if (chunk is not null)
                            lst.Add(chunk);
                    }

                return lst;
            }
        }

        public bool IsChunkLoaded(LocalChunkCoordinates position)
        {
            return _chunks[position.X, position.Z] is not null;
        }

        /// <summary>
        /// Retrieves the requested chunk from the region.
        /// </summary>
        /// <params>
        /// <param name="position">The position of the requested local chunk coordinates.</param>
        /// </params>
        /// <returns>The requested Chunk or null if no Chunk has been saved to disk at this position.</returns>
        public IChunk? GetChunk(LocalChunkCoordinates position)
        {
            return _chunks[position.X, position.Z];
        }

        /// <inheritdoc />
        public IChunk? LoadChunk(LocalChunkCoordinates position)
        {
            int chunkX = position.X;
            int chunkZ = position.Z;

            if (_chunks[chunkX, chunkZ] is not null)
                return _chunks[chunkX, chunkZ];

            Tuple<int, int>? tableEntry = GetChunkFromTable(position);
            if (tableEntry is null)
                return null;

            int chunkDataOffset = tableEntry.Item1;
            var nbt = new NbtFile();
            lock (_streamLock)
            {
                // Add 4 to the chunkDataOffset to skip reading the length.
                _regionFileStream.Seek(chunkDataOffset + 4, SeekOrigin.Begin);
                int compressionMode = _regionFileStream.ReadByte();
                switch (compressionMode)
                {
                    case 1: // gzip
                        throw new NotImplementedException("gzipped chunks are not implemented");
                    case 2: // zlib
                        nbt.LoadFromStream(_regionFileStream, NbtCompression.ZLib, null);
                        break;
                    default:
                        throw new InvalidDataException("Invalid compression scheme provided by region file.");
                }
            }
            IChunk chunk = Chunk.FromNbt(nbt);  // TODO remove dependency on Chunk class
            _chunks[chunkX, chunkZ] = chunk;

            OnChunkLoaded(chunk);

            return chunk;
        }

        protected void OnChunkLoaded(IChunk chunk)
        {
            ChunkLoaded?.Invoke(this, new ChunkLoadedEventArgs(chunk));
        }

        /// <inheritdoc />
        public void AddChunk(IChunk chunk)
        {
            LocalChunkCoordinates position = (LocalChunkCoordinates)chunk.Coordinates;

#if DEBUG
            if (_chunks[position.X, position.Z] is not null)
                throw new ApplicationException("Attempt to add a Chunk that is already loaded.");
#endif

            _chunks[position.X, position.Z] = chunk;
        }

        /// <summary>
        /// Saves this region to the open region file.
        /// </summary>
        public void Save()
        {
            lock (_streamLock)
            {
                for (int x = 0; x < Width; x++)
                    for (int z = 0; z < Depth; z ++)
                    {
                        IChunk? chunk = _chunks[x, z];
                        if (chunk?.IsModified ?? false)
                        {
                            NbtFile data = ((Chunk)chunk).ToNbt();  // TODO remove cast and dependency on Chunk class
                            byte[] raw = data.SaveToBuffer(NbtCompression.ZLib);

                            // Locate/obtain storage for the Chunk
                            LocalChunkCoordinates coords = new LocalChunkCoordinates(x, z);
                            var header = GetChunkFromTable(coords);
                            if (header == null || header.Item2 > raw.Length)
                                header = AllocateNewChunks(coords, raw.Length);

                            // Write the Chunk
                            _regionFileStream.Seek(header.Item1, SeekOrigin.Begin);
                            byte[] rawLength = BitConverter.GetBytes(raw.Length);
                            if (BitConverter.IsLittleEndian)
                                rawLength.Reverse();
                            _regionFileStream.Write(rawLength, 0, rawLength.Length);
                            _regionFileStream.WriteByte(2); // Compressed with zlib
                            _regionFileStream.Write(raw, 0, raw.Length);
                        }
                    }
                _regionFileStream.Flush();
            }
        }

        #region Stream Helpers

        private const int ChunkSizeMultiplier = 4096;
        private byte[] HeaderCache = new byte[8192];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns>
        /// <para>
        /// If there is no Chunk stored at this position, null is returned.
        /// </para>
        /// <para>The first integer of the tuple specifies the offset from the beginning
        /// of the file in bytes.  The second integer specifies the
        /// available space for the Chunk, in bytes.</para>
        /// </returns>
        private Tuple<int, int>? GetChunkFromTable(LocalChunkCoordinates position) // <offset, length>
        {
            int tableOffset = GetTableOffset(position);

            // The first three bytes at location tableOffset give a 24-bit
            // integer which specifies the offset from the beginning of the file
            // where the Chunk is stored.  This offset is in units of 4k "sectors".
            int chunkOffset = HeaderCache[tableOffset] << 16 |
                HeaderCache[tableOffset + 1] << 8 |
                HeaderCache[tableOffset + 2];

            // The length of the Chunk storage in 4k "sectors".
            int length = HeaderCache[tableOffset + 3];

            // Check if the Chunk has never been saved.
            if (chunkOffset == 0 || length == 0)
                return null;

            return new Tuple<int, int>(chunkOffset * ChunkSizeMultiplier,
                length * ChunkSizeMultiplier);
        }

        private void CreateRegionHeader()
        {
            HeaderCache = new byte[8192];
            _regionFileStream.Write(HeaderCache, 0, 8192);
            _regionFileStream.Flush();
        }

        private Tuple<int, int> AllocateNewChunks(LocalChunkCoordinates position, int length)
        {
            lock(_streamLock)
            {
                // Expand region file
                _regionFileStream.Seek(0, SeekOrigin.End);
                int dataOffset = (int)_regionFileStream.Position;

                length /= ChunkSizeMultiplier;
                length++;
                _regionFileStream.Write(new byte[length * ChunkSizeMultiplier], 0, length * ChunkSizeMultiplier);

                // Write table entry
                int tableOffset = GetTableOffset(position);
                _regionFileStream.Seek(tableOffset, SeekOrigin.Begin);

                byte[] entry = BitConverter.GetBytes(dataOffset >> 4);
                entry[0] = (byte)length;
                Array.Reverse(entry);
                _regionFileStream.Write(entry, 0, entry.Length);
                Buffer.BlockCopy(entry, 0, HeaderCache, tableOffset, 4);

                return new Tuple<int, int>(dataOffset, length * ChunkSizeMultiplier);
            }
        }

        /// <summary>
        /// Gets the offset (in bytes) of the Chunk location and sector count
        /// information within the Header table.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private int GetTableOffset(LocalChunkCoordinates pos)
        {
            return (pos.X + pos.Z * Width) * 4;
        }

        #endregion

        public static string GetRegionFileName(RegionCoordinates position)
        {
            return string.Format("r.{0}.{1}.mcr", position.X, position.Z);
        }

        /// <summary>
        /// Determines whether or not a Region File already exists for the given
        /// Region.
        /// </summary>
        /// <param name="position">The Region Coordinates to check.</param>
        /// <param name="baseDirectory">The path to the containing Dimension's
        /// files.</param>
        /// <returns>True if the file exists; false otherwise.</returns>
        public static bool DoesRegionExistOnDisk(RegionCoordinates position, string baseDirectory)
        {
            string filename = Path.Combine(baseDirectory, "region", GetRegionFileName(position));
            return File.Exists(filename);
        }

        public void Dispose()
        {
            if (_regionFileStream is null)
                return;

            _regionFileStream.Flush();
            _regionFileStream.Close();

            // It's illegal to use a disposed object, so we can set
            // these to null safely.
            _regionFileStream = null!;
            _streamLock = null!;
        }
    }
}
