﻿using System;

namespace TrueCraft.Core.Logic
{
    /// <summary>
    /// Providers item providers for a server.
    /// </summary>
    public interface IItemRepository
    {
        /// <summary>
        /// Gets this repository's item provider for the specified item ID. This may return null
        /// if the item ID in question has no corresponding block provider.
        /// </summary>
        IItemProvider? GetItemProvider(short id);
    }
}
