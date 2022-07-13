using System;
using System.Collections.Generic;
using TrueCraft;
using TrueCraft.Core.Server;

namespace TrueCraft.World
{
    public interface IDimensionFactory
    {
        /// <summary>
        /// Builds a set of dimensions for a World.
        /// </summary>
        /// <param name="serviceLocator"></param>
        /// <param name="baseDirectory">The folder containing the World Saves.</param>
        /// <param name="seed">The seed for generating the World.</param>
        /// <returns></returns>
        IList<IDimensionServer> BuildDimensions(IServerServiceLocator serviceLocator, string baseDirectory, int seed);
    }
}
