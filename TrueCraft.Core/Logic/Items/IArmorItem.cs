﻿namespace TrueCraft.Core.Logic.Items
{
    public interface IArmorItem : IItemProvider
    {
        /// <summary>
        /// Gets the Kind of Armor represented by this ArmorItem.
        /// </summary>
        ArmorKind Kind { get; }

        /// <summary>
        /// Gets the Material from which the ArmorItem is made.
        /// </summary>
        ArmorMaterial Material { get; }

        /// <summary>
        /// Gets the Durability of the ArmorItem when newly crafted.
        /// </summary>
        short Durability { get; }

        /// <summary>
        /// Gets the number of Defence Points of protection provided by the ArmorItem.
        /// </summary>
        float DefencePoints { get; }
    }
}