using System;
using TrueCraft.Core.Server;
using TrueCraft.World;

namespace TrueCraft
{
    public interface IServerServiceLocator : IServiceLocator
    {
        /// <summary>
        /// Gets or sets the World of this Server.
        /// </summary>
        /// <remarks>
        /// The World may only be set once.
        /// </remarks>
        IWorld World { get; set; }

        /// <summary>
        /// 
        /// </summary>
        IMultiplayerServer Server { get; }
    }
}
