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

        private HashSet<LocalChunkCoordinates> DirtyChunks { get; } = new HashSet<LocalChunkCoordinates>(Width * Depth);

        private Stream _regionFile;
        private object _streamLock = new object();

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

            string filename = Path.Combine(baseDirectory, "region", GetRegionFileName(position));

            if (File.Exists(filename))
            {
                _regionFile = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                _regionFile.Read(HeaderCache, 0, 8192);
            }
            else
            {
                _regionFile = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
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
            int chunkX = position.X;
            int chunkZ = position.Z;

            if (_chunks[chunkX, chunkZ] is not null)
                return _chunks[chunkX, chunkZ];

            Tuple<int, int>? tableEntry = GetChunkFromTable(position);
            if (tableEntry is null)
                return null;

            int chunkDataOffset = tableEntry.Item1;
            NbtFile nbt = new NbtFile();
            lock (_streamLock)
            {
                // Add 4 to the chunkDataOffset to skip reading the length.
                _regionFile.Seek(chunkDataOffset + 4, SeekOrigin.Begin);
                int compressionMode = _regionFile.ReadByte();
                switch (compressionMode)
                {
                    case 1: // gzip
                        throw new NotImplementedException("gzipped chunks are not implemented");
                    case 2: // zlib
                        nbt.LoadFromStream(_regionFile, NbtCompression.ZLib, null);
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
        /// Saves this region to the specified file.
        /// </summary>
        public void Save(string file)
        {
            if(File.Exists(file))
                _regionFile = _regionFile ?? File.Open(file, FileMode.OpenOrCreate);
            else
            {
                _regionFile = _regionFile ?? File.Open(file, FileMode.OpenOrCreate);
                CreateRegionHeader();
            }
            Save();
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
                            _regionFile.Seek(header.Item1, SeekOrigin.Begin);
                            byte[] rawLength = BitConverter.GetBytes(raw.Length);
                            if (BitConverter.IsLittleEndian)
                                rawLength.Reverse();
                            _regionFile.Write(rawLength, 0, rawLength.Length);
                            _regionFile.WriteByte(2); // Compressed with zlib
                            _regionFile.Write(raw, 0, raw.Length);
                        }
                    }
                _regionFile.Flush();
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
            _regionFile.Write(HeaderCache, 0, 8192);
            _regionFile.Flush();
        }

        private Tuple<int, int> AllocateNewChunks(LocalChunkCoordinates position, int length)
        {
            // Expand region file
            _regionFile.Seek(0, SeekOrigin.End);
            int dataOffset = (int)_regionFile.Position;

            length /= ChunkSizeMultiplier;
            length++;
            _regionFile.Write(new byte[length * ChunkSizeMultiplier], 0, length * ChunkSizeMultiplier);

            // Write table entry
            int tableOffset = GetTableOffset(position);
            _regionFile.Seek(tableOffset, SeekOrigin.Begin);

            byte[] entry = BitConverter.GetBytes(dataOffset >> 4);
            entry[0] = (byte)length;
            Array.Reverse(entry);
            _regionFile.Write(entry, 0, entry.Length);
            Buffer.BlockCopy(entry, 0, HeaderCache, tableOffset, 4);

            return new Tuple<int, int>(dataOffset, length * ChunkSizeMultiplier);
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

        public void Dispose()
        {
            if (_regionFile == null)
                return;
            lock (_streamLock)
            {
                _regionFile.Flush();
                _regionFile.Close();
            }
        }
    }
}
