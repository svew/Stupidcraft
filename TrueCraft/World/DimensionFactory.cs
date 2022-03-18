using System;
using System.Collections.Generic;
using TrueCraft.Core;
using TrueCraft.Core.TerrainGen;
using TrueCraft.Core.World;

namespace TrueCraft.World
{
    public class DimensionFactory : IDimensionFactory
    {
        public DimensionFactory()
        {
        }

        /// <inheritdoc />
        public IList<IDimension> BuildDimensions(string baseDirectory, int seed)
        {
            List<IDimension> dimensions = new List<IDimension>(2);

            dimensions.Add(null);   // TODO nether

            IChunkProvider chunkProvider = new StandardGenerator(seed);
            IDimension overWorld = new Dimension(baseDirectory, "OverWorld", chunkProvider);
            dimensions.Add(overWorld);

            return dimensions;
        }
    }
}
