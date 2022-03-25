using System;
using System.Collections.Generic;
using TrueCraft.Core;
using TrueCraft.Core.Lighting;
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
            ILightingQueue lightingQueue = new LightingQueue();
            IDimensionServer overWorld = new Dimension(baseDirectory, DimensionID.Overworld, chunkProvider, lightingQueue, BlockRepository.Get());
            dimensions.Add(overWorld);

            // TODO Lighting Queue needs to be hooked up to its consumer.

            return dimensions;
        }
    }
}
