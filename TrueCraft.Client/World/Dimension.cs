using System;
using System.Collections;
using System.Collections.Generic;
using TrueCraft.Core.Logic;
using TrueCraft.Core.World;

namespace TrueCraft.Client.World
{
    public class Dimension : IDimensionClient
    {
        private readonly IBlockRepository _blockRepository;

        private readonly IItemRepository _itemRepository;

        private Dictionary<GlobalChunkCoordinates, IChunk> _chunks;

        public Dimension(IBlockRepository blockRepository, IItemRepository itemRepository)
        {
            _blockRepository = blockRepository;
            _itemRepository = itemRepository;
            _chunks = new Dictionary<GlobalChunkCoordinates, IChunk>(434);
        }

        /// <inheritdoc />
        public DimensionID ID { get => DimensionID.Overworld; }

        /// <inheritdoc />
        public string Name
        {
            get
            {
                return DimensionInfo.GetName(ID);
            }
        }

        /// <inheritdoc />
        public IBlockRepository BlockRepository { get => _blockRepository; }

        /// <inheritdoc />
        public IItemRepository ItemRepository { get => _itemRepository; }

        /// <inheritdoc />
        public long TimeOfDay { get; set; }

        /// <inheritdoc />
        public IChunk? GetChunk(GlobalVoxelCoordinates coordinates)
        {
            return GetChunk((GlobalChunkCoordinates)coordinates);
        }

        /// <inheritdoc />
        public IChunk? GetChunk(GlobalChunkCoordinates coordinates)
        {
            if (_chunks.ContainsKey(coordinates))
                return _chunks[coordinates];

            return null;
        }

        /// <inheritdoc />
        public void AddChunk(IChunk chunk)
        {
            _chunks[chunk.Coordinates] = chunk;
        }

        /// <inheritdoc />
        public LocalVoxelCoordinates FindBlockPosition(GlobalVoxelCoordinates coordinates, out IChunk? chunk)
        {
            chunk = GetChunk(coordinates);
            return (LocalVoxelCoordinates)coordinates;
        }

        /// <inheritdoc />
        public BlockDescriptor GetBlockData(GlobalVoxelCoordinates coordinates)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public byte GetBlockID(GlobalVoxelCoordinates coordinates)
        {
            IChunk? chunk = GetChunk(coordinates);
            return chunk?.GetBlockID((LocalVoxelCoordinates)coordinates) ?? 0;
        }

        /// <inheritdoc />
        public byte GetBlockLight(GlobalVoxelCoordinates coordinates)
        {
            IChunk? chunk = GetChunk(coordinates);
            return chunk?.GetBlockLight((LocalVoxelCoordinates)coordinates) ?? 0;
        }

        /// <inheritdoc />
        public byte GetMetadata(GlobalVoxelCoordinates coordinates)
        {
            IChunk? chunk = GetChunk(coordinates);
            return chunk?.GetMetadata((LocalVoxelCoordinates)coordinates) ?? 0;
        }

        /// <inheritdoc />
        public byte GetSkyLight(GlobalVoxelCoordinates coordinates)
        {
            IChunk? chunk = GetChunk(coordinates);
            return chunk?.GetSkyLight((LocalVoxelCoordinates)coordinates) ?? 0;
        }

        /// <inheritdoc />
        public bool IsChunkLoaded(GlobalVoxelCoordinates coordinates)
        {
            return _chunks.ContainsKey((GlobalChunkCoordinates)coordinates);
        }

        /// <inheritdoc />
        public bool IsValidPosition(GlobalVoxelCoordinates position)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SetBlockData(GlobalVoxelCoordinates coordinates, BlockDescriptor block)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SetBlockID(GlobalVoxelCoordinates coordinates, byte value)
        {
            IChunk? chunk = GetChunk(coordinates);
            chunk?.SetBlockID((LocalVoxelCoordinates)coordinates, value);
        }

        /// <inheritdoc />
        public void SetBlockLight(GlobalVoxelCoordinates coordinates, byte value)
        {
            IChunk? chunk = GetChunk(coordinates);
            chunk?.SetBlockLight((LocalVoxelCoordinates)coordinates, value);
        }

        /// <inheritdoc />
        public void SetMetadata(GlobalVoxelCoordinates coordinates, byte value)
        {
            IChunk? chunk = GetChunk(coordinates);
            chunk?.SetMetadata((LocalVoxelCoordinates)coordinates, value);
        }

        /// <inheritdoc />
        public void SetSkyLight(GlobalVoxelCoordinates coordinates, byte value)
        {
            IChunk? chunk = GetChunk(coordinates);
            chunk?.SetSkyLight((LocalVoxelCoordinates)coordinates, value);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _chunks.Values.GetEnumerator();
        }

        /// <inheritdoc />
        public IEnumerator<IChunk> GetEnumerator()
        {
            return _chunks.Values.GetEnumerator();
        }
    }
}
