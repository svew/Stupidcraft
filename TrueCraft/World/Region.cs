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
    /// Not all of these chunks are represented at any given time, and
    /// will be loaded from disk or generated when the need arises.
    /// </summary>
    public class Region : IDisposable, IRegion
    {
        /// <summary>
        /// The number of chunks within the region in the X-direction.
        /// </summary>
        public const int Width = WorldConstants.RegionWidth;

        /// <summary>
        /// The number of chunks within the region in the Z-direction.
        /// </summary>
        public const int Depth = WorldConstants.RegionDepth;

        private ConcurrentDictionary<LocalChunkCoordinates, IChunk> _chunks { get; }

        public IEnumerable<IChunk> Chunks { get => _chunks.Values;  }

        /// <summary>
        /// The location of this region in the overworld.
        /// </summary>
        public RegionCoordinates Position { get; }

        public Dimension Dimension { get; }  // TODO should be IDimension

        private HashSet<LocalChunkCoordinates> DirtyChunks { get; } = new HashSet<LocalChunkCoordinates>(Width * Depth);

        private Stream _regionFile;
        private object _streamLock = new object();

        /// <summary>
        /// Creates a new Region for server-side use at the given position in
        /// the given World.
        /// </summary>
        /// <params>
        /// <param name="position"></param>
        /// <param name="dimension"></param>
        /// </params>
        public Region(RegionCoordinates position, IDimension dimension)
        {
            _chunks = new ConcurrentDictionary<LocalChunkCoordinates, IChunk>(Environment.ProcessorCount, Width * Depth);
            Position = position;
            Dimension = (Dimension)dimension;
        }

        /// <summary>
        /// Creates a region from the given region file.
        /// </summary>
        public Region(RegionCoordinates position, IDimension dimension, string file) : this(position, dimension)
        {
            if (File.Exists(file))
            {
                _regionFile = File.Open(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                _regionFile.Read(HeaderCache, 0, 8192);
            }
            else
            {
                _regionFile = File.Open(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                CreateRegionHeader();
            }
        }

        public void DamageChunk(LocalChunkCoordinates coords)
        {
            DirtyChunks.Add(coords);
        }

        public bool IsChunkLoaded(LocalChunkCoordinates position)
        {
            return _chunks.ContainsKey(position);
        }

        /// <summary>
        /// Retrieves the requested chunk from the region, or
        /// generates it if a world generator is provided.
        /// </summary>
        /// <params>
        /// <param name="position">The position of the requested local chunk coordinates.</param>
        /// </params>
        public IChunk GetChunk(LocalChunkCoordinates position, bool generate = true)
        {
            if (!_chunks.ContainsKey(position))
            {
                if (_regionFile != null)
                {
                    // Search the stream for that region
                    var chunkData = GetChunkFromTable(position);
                    if (chunkData == null)
                    {
                        if (Dimension.ChunkProvider == null)
                            throw new ArgumentException("The requested chunk is not loaded.", "position");
                        if (generate)
                            GenerateChunk(position);
                        else
                            return null;
                        // TODO BUG On client exit, an exception was thrown here
                        //     due to position not being in the Dictionary.
                        return _chunks[position];
                    }

                    NbtFile nbt = new NbtFile();
                    lock (_streamLock)
                    {
                        _regionFile.Seek(chunkData.Item1, SeekOrigin.Begin);
                        /*int length = */
                        new MinecraftStream(_regionFile).ReadInt32(); // TODO: Avoid making new objects here, and in the WriteInt32
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
                    IChunk chunk = Chunk.FromNbt(nbt);
                    chunk.ParentRegion = this;
                    _chunks[position] = chunk;
                    // TODO: NOTE OnChunkLoaded will cause a lighting operation to be queued for this chunk.
                    //    There are 2 threads enqueuing and dequeueing lighting operations (DoEnvironment,
                    //    and the thread responding to the login packet).  Such lighting operations would
                    //    then cross over from one thread to another, leading to each thread trying to obtain
                    //    a lock owned by the other - a deadlock.  Moving the lighting operations outside the
                    //    lock is to resolve this.
                    Dimension.OnChunkLoaded(new ChunkLoadedEventArgs(chunk));
                }
                else if (Dimension.ChunkProvider == null)
                    throw new ArgumentException("The requested chunk is not loaded.", nameof(position));
                else
                {
                    if (generate)
                        GenerateChunk(position);
                    else
                        return null;
                }
            }
            // TODO BUG: Just after Client exit, this threw an exception because
            //    position was not in the Dictionary.  It's happened for Grass
            //    growing and also for expanding the Client's Chunk Radius (exit
            //    was nearly immediately after entry, so the chunk radius was not
            //    fully expanded).
            return _chunks[position];
        }

        public void GenerateChunk(LocalChunkCoordinates position)
        {
            GlobalChunkCoordinates globalPosition = this.Position.GetGlobalChunkCoordinates(position);
            var chunk = Dimension.ChunkProvider.GenerateChunk(globalPosition);
            chunk.IsModified = true;
            chunk.ParentRegion = this;
            DirtyChunks.Add(position);
            _chunks[position] = chunk;
            Dimension.OnChunkGenerated(new ChunkLoadedEventArgs(chunk));
        }

        /// <summary>
        /// Sets the chunk at the specified local position to the given value.
        /// </summary>
        public void SetChunk(LocalChunkCoordinates position, IChunk chunk)
        {
            _chunks[position] = chunk;
            chunk.IsModified = true;
            DirtyChunks.Add(position);
            chunk.ParentRegion = this;
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
                var toRemove = new List<LocalChunkCoordinates>();
                var chunks = DirtyChunks.ToList();
                DirtyChunks.Clear();
                foreach (var coords in chunks)
                {
                    var chunk = GetChunk(coords, generate: false);
                    if (chunk.IsModified)
                    {
                        var data = ((Chunk)chunk).ToNbt();
                        byte[] raw = data.SaveToBuffer(NbtCompression.ZLib);

                        var header = GetChunkFromTable(coords);
                        if (header == null || header.Item2 > raw.Length)
                            header = AllocateNewChunks(coords, raw.Length);

                        _regionFile.Seek(header.Item1, SeekOrigin.Begin);
                        new MinecraftStream(_regionFile).WriteInt32(raw.Length);
                        _regionFile.WriteByte(2); // Compressed with zlib
                        _regionFile.Write(raw, 0, raw.Length);

                        chunk.IsModified = false;
                    }
                    if ((DateTime.UtcNow - chunk.LastAccessed).TotalMinutes > 5)
                        toRemove.Add(coords);
                }
                _regionFile.Flush();
                // Unload idle chunks
                foreach (var chunk in toRemove)
                {
                    IChunk c;
                    _chunks.Remove(chunk, out c);
                    c.Dispose();
                }
            }
        }

        #region Stream Helpers

        private const int ChunkSizeMultiplier = 4096;
        private byte[] HeaderCache = new byte[8192];
        
        private Tuple<int, int> GetChunkFromTable(LocalChunkCoordinates position) // <offset, length>
        {
            int tableOffset = GetTableOffset(position);
            byte[] offsetBuffer = new byte[4];
            Buffer.BlockCopy(HeaderCache, tableOffset, offsetBuffer, 0, 3);
            Array.Reverse(offsetBuffer);
            int length = HeaderCache[tableOffset + 3];
            int offset = BitConverter.ToInt32(offsetBuffer, 0) << 4;
            if (offset == 0 || length == 0)
                return null;
            return new Tuple<int, int>(offset,
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

        private int GetTableOffset(LocalChunkCoordinates pos)
        {
            return (pos.X + pos.Z * Width) * 4;
        }

        #endregion

        public static string GetRegionFileName(RegionCoordinates position)
        {
            return string.Format("r.{0}.{1}.mca", position.X, position.Z);
        }

        public void UnloadChunk(LocalChunkCoordinates position)
        {
            IChunk c;
            _chunks.Remove(position, out c);
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
