using System;
using System.Collections.Generic;
using TrueCraft.Core.World;

namespace TrueCraft.Core.TerrainGen
{
    public abstract class Generator : IChunkProvider
    {
        protected readonly int _seed;

        private readonly List<IChunkDecorator> _decorators;

        protected Generator(int seed)
        {
            _seed = seed;
            _decorators = new List<IChunkDecorator>();
        }

        protected IList<IChunkDecorator> ChunkDecorators { get => _decorators; }

        public abstract IChunk GenerateChunk(GlobalChunkCoordinates coordinates);

        public abstract GlobalVoxelCoordinates GetSpawn(IDimension dimension);
    }
}
