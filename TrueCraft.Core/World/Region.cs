using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using fNbt;
using Ionic.Zlib;
using TrueCraft.API;
using TrueCraft.API.World;
using TrueCraft.Core.Networking;

namespace TrueCraft.Core.World
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

        private ConcurrentDictionary<LocalChunkCoordinates, IChunk> _Chunks { get; }

        /// <summary>
        /// The location of this region in the overworld.
        /// </summary>
        public RegionCoordinates Position { get; }

        public World World { get; }

        private HashSet<LocalChunkCoordinates> DirtyChunks { get; } = new HashSet<LocalChunkCoordinates>(Width * Depth);

        private Stream regionFile { get; set; }
        private object streamLock = new object();

        /// <summary>
        /// Creates a new Region for server-side use at the given position in
        /// the given World.
        /// </summary>
        /// <params>
        /// <param name="position"></param>
        /// <param name="world"></param>
        /// </params>
        public Region(RegionCoordinates position, World world)
        {
            _Chunks = new ConcurrentDictionary<LocalChunkCoordinates, IChunk>(Environment.ProcessorCount, Width * Depth);
            Position = position;
            World = world;
        }

        /// <summary>
        /// Creates a region from the given region file.
        /// </summary>
        public Region(RegionCoordinates position, World world, string file) : this(position, world)
        {
            if (File.Exists(file))
            {
                regionFile = File.Open(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                regionFile.Read(HeaderCache, 0, 8192);
            }
            else
            {
                regionFile = File.Open(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                CreateRegionHeader();
            }
        }

        public void DamageChunk(LocalChunkCoordinates coords)
        {
            DirtyChunks.Add(coords);
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
            if (!Chunks.ContainsKey(position))
            {
                if (regionFile != null)
                {
                    // Search the stream for that region
                    var chunkData = GetChunkFromTable(position);
                    if (chunkData == null)
                    {
                        if (World.ChunkProvider == null)
                            throw new ArgumentException("The requested chunk is not loaded.", "position");
                        if (generate)
                            GenerateChunk(position);
                        else
                            return null;
                        return Chunks[position];
                    }
                    lock (streamLock)
                    {
                        regionFile.Seek(chunkData.Item1, SeekOrigin.Begin);
                        /*int length = */
                        new MinecraftStream(regionFile).ReadInt32(); // TODO: Avoid making new objects here, and in the WriteInt32
                        int compressionMode = regionFile.ReadByte();
                        switch (compressionMode)
                        {
                            case 1: // gzip
                                throw new NotImplementedException("gzipped chunks are not implemented");
                            case 2: // zlib
                                var nbt = new NbtFile();
                                nbt.LoadFromStream(regionFile, NbtCompression.ZLib, null);
                                var chunk = Chunk.FromNbt(nbt);
                                chunk.ParentRegion = this;
                                Chunks[position] = chunk;
                                World.OnChunkLoaded(new ChunkLoadedEventArgs(chunk));
                                break;
                            default:
                                throw new InvalidDataException("Invalid compression scheme provided by region file.");
                        }
                    }
                }
                else if (World.ChunkProvider == null)
                    throw new ArgumentException("The requested chunk is not loaded.", nameof(position));
                else
                {
                    if (generate)
                        GenerateChunk(position);
                    else
                        return null;
                }
            }
            return Chunks[position];
        }

        public void GenerateChunk(LocalChunkCoordinates position)
        {
            GlobalChunkCoordinates globalPosition = this.Position.GetGlobalChunkCoordinates(position);
            var chunk = World.ChunkProvider.GenerateChunk(World, globalPosition);
            chunk.IsModified = true;
            chunk.Coordinates = globalPosition;
            chunk.ParentRegion = this;
            DirtyChunks.Add(position);
            Chunks[position] = chunk;
            World.OnChunkGenerated(new ChunkLoadedEventArgs(chunk));
        }

        /// <summary>
        /// Sets the chunk at the specified local position to the given value.
        /// </summary>
        public void SetChunk(LocalChunkCoordinates position, IChunk chunk)
        {
            Chunks[position] = chunk;
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
                regionFile = regionFile ?? File.Open(file, FileMode.OpenOrCreate);
            else
            {
                regionFile = regionFile ?? File.Open(file, FileMode.OpenOrCreate);
                CreateRegionHeader();
            }
            Save();
        }

        /// <summary>
        /// Saves this region to the open region file.
        /// </summary>
        public void Save()
        {
            lock (streamLock)
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

                        regionFile.Seek(header.Item1, SeekOrigin.Begin);
                        new MinecraftStream(regionFile).WriteInt32(raw.Length);
                        regionFile.WriteByte(2); // Compressed with zlib
                        regionFile.Write(raw, 0, raw.Length);

                        chunk.IsModified = false;
                    }
                    if ((DateTime.UtcNow - chunk.LastAccessed).TotalMinutes > 5)
                        toRemove.Add(coords);
                }
                regionFile.Flush();
                // Unload idle chunks
                foreach (var chunk in toRemove)
                {
                    var c = Chunks[chunk];
                    Chunks.Remove(chunk);
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
            regionFile.Write(HeaderCache, 0, 8192);
            regionFile.Flush();
        }

        private Tuple<int, int> AllocateNewChunks(LocalChunkCoordinates position, int length)
        {
            // Expand region file
            regionFile.Seek(0, SeekOrigin.End);
            int dataOffset = (int)regionFile.Position;

            length /= ChunkSizeMultiplier;
            length++;
            regionFile.Write(new byte[length * ChunkSizeMultiplier], 0, length * ChunkSizeMultiplier);

            // Write table entry
            int tableOffset = GetTableOffset(position);
            regionFile.Seek(tableOffset, SeekOrigin.Begin);

            byte[] entry = BitConverter.GetBytes(dataOffset >> 4);
            entry[0] = (byte)length;
            Array.Reverse(entry);
            regionFile.Write(entry, 0, entry.Length);
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
            Chunks.Remove(position);
        }

        public void Dispose()
        {
            if (regionFile == null)
                return;
            lock (streamLock)
            {
                regionFile.Flush();
                regionFile.Close();
            }
        }
    }
}
