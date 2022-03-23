using System;
using System.Collections.Generic;
using TrueCraft.Core;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;
using TrueCraft.TerrainGen;

namespace TrueCraft.World
{
    public class DimensionFactory : IDimensionFactory
    {
        public DimensionFactory()
        {
        }

        /// <inheritdoc />
        public IList<IDimensionServer> BuildDimensions(string baseDirectory, int seed)
        {
            List<IDimensionServer> dimensions = new List<IDimensionServer>(2);

            dimensions.Add(null!);   // TODO nether

            IChunkProvider chunkProvider = new StandardGenerator(seed);
            IDimensionServer overWorld = new Dimension(baseDirectory, DimensionID.Overworld, chunkProvider, BlockRepository.Get());
            dimensions.Add(overWorld);

            return dimensions;
        }
    }
}
