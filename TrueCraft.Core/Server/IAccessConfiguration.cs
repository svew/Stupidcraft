﻿using System.Collections.Generic;
using TrueCraft.Core.Networking;

namespace TrueCraft.Core
{
    /// <summary>
    /// Interface for objects providing server access configuration.
    /// </summary>
    public interface IAccessConfiguration
    {
        /// <summary>
        /// Gets a list of blacklisted players for the configuration.
        /// </summary>
        IList<string> Blacklist { get; }

        /// <summary>
        /// Gets a list of whitelisted players for the configuration.
        /// </summary>
        IList<string> Whitelist { get; }

        /// <summary>
        /// Gets a list of opped players for the configuration.
        /// </summary>
        IList<string> Oplist { get; }
    }
}
