﻿using System;

namespace TrueCraft.Core.Logic
{
    /// <summary>
    /// Describes an item that can be smelted in a furnace to produce a new item.
    /// </summary>
    public interface ISmeltableItem
    {
        /// <summary>
        /// The item this becomes when smelted.
        /// </summary>
        ItemStack SmeltingOutput { get; }
    }
}
