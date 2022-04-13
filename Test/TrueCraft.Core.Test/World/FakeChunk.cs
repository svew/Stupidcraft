using System;
using fNbt;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Test.World
{
    // TODO: This "unit" test depends upon various BlockProvider subclasses for Block ID values.
    public class FakeChunk : IChunk
    {
        private byte[] _blocks;
        private byte[] _blockLight;
        private byte[] _skyLight;
        private byte[] _metadata;
        private byte[,] _heightMap;

        public FakeChunk(GlobalChunkCoordinates coordinates)
        {
            Coordinates = coordinates;

            _blocks = new byte[WorldConstants.ChunkDepth * WorldConstants.ChunkWidth * WorldConstants.Height];
            _blockLight = new byte[WorldConstants.ChunkDepth * WorldConstants.ChunkWidth * WorldConstants.Height];
            _skyLight = new byte[WorldConstants.ChunkDepth * WorldConstants.ChunkWidth * WorldConstants.Height];
            _metadata = new byte[WorldConstants.ChunkDepth * WorldConstants.ChunkWidth * WorldConstants.Height];
            _heightMap = new byte[WorldConstants.ChunkWidth, WorldConstants.ChunkDepth];

            for (int x = 0; x < WorldConstants.ChunkWidth; x ++)
                for (int z = 0; z < WorldConstants.ChunkDepth; z ++)
                {
                    _blocks[(x * WorldConstants.ChunkWidth + z) * WorldConstants.ChunkDepth] = BedrockBlock.BlockID;
                    for (int y = 1; y < 5; y ++)
                        _blocks[(x * WorldConstants.ChunkWidth + z) * WorldConstants.ChunkDepth + y] = DirtBlock.BlockID;
                    _heightMap[x, z] = 5;
                }
        }

        public int X { get => Coordinates.X; }

        public int Z { get => Coordinates.Z; }

        public GlobalChunkCoordinates Coordinates { get; }

        private int CoordinatesToIndex(LocalVoxelCoordinates coordinates)
        {
            return (coordinates.X * WorldConstants.ChunkWidth + coordinates.Z) * WorldConstants.Height + coordinates.Y;
        }

        public int MaxHeight => throw new NotImplementedException();

        public bool IsModified => throw new NotImplementedException();

        public bool LightPopulated { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public DateTime LastAccessed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public byte[] Data => throw new NotImplementedException();

        public bool TerrainPopulated { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public NybbleArray Metadata => throw new NotImplementedException();

        public NybbleArray BlockLight => throw new NotImplementedException();

        public NybbleArray SkyLight => throw new NotImplementedException();

        public event EventHandler? Disposed;

        public void Dispose()
        {
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        public Biome GetBiome(int x, int z)
        {
            throw new NotImplementedException();
        }

        public byte GetBlockID(LocalVoxelCoordinates coordinates)
        {
            return _blocks[CoordinatesToIndex(coordinates)];
        }

        public byte GetBlockLight(LocalVoxelCoordinates coordinates)
        {
            return _blockLight[CoordinatesToIndex(coordinates)];
        }

        public int GetHeight(int x, int z)
        {
            throw new NotImplementedException();
        }

        public byte GetMetadata(LocalVoxelCoordinates coordinates)
        {
            return _metadata[CoordinatesToIndex(coordinates)];
        }

        public byte GetSkyLight(LocalVoxelCoordinates coordinates)
        {
            return _skyLight[CoordinatesToIndex(coordinates)];
        }

        public NbtCompound GetTileEntity(LocalVoxelCoordinates coordinates)
        {
            throw new NotImplementedException();
        }

        public void SetBlockID(LocalVoxelCoordinates coordinates, byte value)
        {
            _blocks[CoordinatesToIndex(coordinates)] = value;

            if (coordinates.Y == _heightMap[coordinates.X, coordinates.Z] && value == AirBlock.BlockID)
            {
                int y = coordinates.Y;
                while (y >= 0 && AirBlock.BlockID == GetBlockID(new LocalVoxelCoordinates(coordinates.X, y, coordinates.Z)))
                    y--;
                if (y < 0) y = 0;
                _heightMap[coordinates.X, coordinates.Z] = (byte)y;
            }
            if (coordinates.Y > _heightMap[coordinates.X, coordinates.Z] && value != AirBlock.BlockID)
                _heightMap[coordinates.X, coordinates.Z] = (byte)coordinates.Y;
        }

        public void SetBlockLight(LocalVoxelCoordinates coordinates, byte value)
        {
            _blockLight[CoordinatesToIndex(coordinates)] = value;
        }

        public void SetMetadata(LocalVoxelCoordinates coordinates, byte value)
        {
            _metadata[CoordinatesToIndex(coordinates)] = value;
        }

        public void SetSkyLight(LocalVoxelCoordinates coordinates, byte value)
        {
            _skyLight[CoordinatesToIndex(coordinates)] = value;
        }

        public void SetTileEntity(LocalVoxelCoordinates coordinates, NbtCompound value)
        {
            throw new NotImplementedException();
        }

        public void UpdateHeightMap()
        {
            throw new NotImplementedException("Fix call to obsolete method.");
        }
    }
}
