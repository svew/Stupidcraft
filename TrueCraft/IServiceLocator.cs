using System;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;
using TrueCraft.World;

namespace TrueCraft
{
    public interface IServiceLocator
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

        /// <summary>
        /// Gets the Block Repository.
        /// </summary>
        IBlockRepository BlockRepository { get; }

        /// <summary>
        /// Gets the Item Repository.
        /// </summary>
        IItemRepository ItemRepository { get; }
    }
}
