using System;
using System.Collections.ObjectModel;
using TrueCraft.Core;
using TrueCraft.Core.World;

namespace TrueCraft.Client
{
    public class ReadOnlyWorld
    {
        private bool UnloadChunks { get; set; }

        internal Dimension World { get; set; }

        public long Time { get { return World.Time; } }

        internal ReadOnlyWorld()
        {
            World = new Dimension("default");
            UnloadChunks = true;
        }

        public byte GetBlockID(GlobalVoxelCoordinates coordinates)
        {
            return World.GetBlockID(coordinates);
        }

        internal void SetBlockID(GlobalVoxelCoordinates coordinates, byte value)
        {
          World.SetBlockID(coordinates, value);
        }

        internal void SetMetadata(GlobalVoxelCoordinates coordinates, byte value)
        {
          World.SetMetadata(coordinates, value);
        }

        public byte GetMetadata(GlobalVoxelCoordinates coordinates)
        {
            return World.GetMetadata(coordinates);
        }

        public byte GetSkyLight(GlobalVoxelCoordinates coordinates)
        {
            return World.GetSkyLight(coordinates);
        }

        internal IChunk FindChunk(GlobalColumnCoordinates coordinates)
        {
            try
            {
                return World.FindChunk(new GlobalVoxelCoordinates(coordinates.X, 0, coordinates.Z));
            }
            catch
            {
                return null;
            }
        }

        public ReadOnlyChunk GetChunk(GlobalChunkCoordinates coordinates)
        {
            return new ReadOnlyChunk(World.GetChunk(coordinates));
        }

        internal void SetChunk(GlobalChunkCoordinates coordinates, Chunk chunk)
        {
            World.SetChunk(coordinates, chunk);
        }

        internal void RemoveChunk(GlobalChunkCoordinates coordinates)
        {
            if (UnloadChunks)
                World.UnloadChunk(coordinates);
        }

        public bool IsValidPosition(GlobalVoxelCoordinates coords)
        {
            return World.IsValidPosition(coords);
        }
    }

    public class ReadOnlyChunk
    {
        internal IChunk Chunk { get; set; }

        internal ReadOnlyChunk(IChunk chunk)
        {
            Chunk = chunk;
        }

        public byte GetBlockId(LocalVoxelCoordinates coordinates)
        {
            return Chunk.GetBlockID(coordinates);
        }

        public byte GetMetadata(LocalVoxelCoordinates coordinates)
        {
            return Chunk.GetMetadata(coordinates);
        }

        public byte GetSkyLight(LocalVoxelCoordinates coordinates)
        {
            return Chunk.GetSkyLight(coordinates);
        }

        public byte GetBlockLight(LocalVoxelCoordinates coordinates)
        {
            return Chunk.GetBlockLight(coordinates);
        }

        public GlobalChunkCoordinates Coordinates { get { return Chunk.Coordinates; } }

        public int X { get { return Chunk.X; } }
        public int Z { get { return Chunk.Z; } }

        public ReadOnlyCollection<byte> Blocks { get { return Array.AsReadOnly(Chunk.Data); } }
        public ReadOnlyNibbleArray Metadata { get { return new ReadOnlyNibbleArray(Chunk.Metadata); } }
        public ReadOnlyNibbleArray BlockLight { get { return new ReadOnlyNibbleArray(Chunk.BlockLight); } }
        public ReadOnlyNibbleArray SkyLight { get { return new ReadOnlyNibbleArray(Chunk.SkyLight); } }
    }
}
