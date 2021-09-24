using System;
using TrueCraft.API.Server;
using TrueCraft.API.Windows;
using TrueCraft.API.World;

namespace TrueCraft.Core.Windows
{
    public interface IFurnaceWindowContent : IWindowContent
    {
        ISlots Ingredient { get; }

        ISlots Fuel { get; }

        ISlots Output { get; }

        /// <summary>
        /// Gets the location of the Furnace.
        /// </summary>
        GlobalVoxelCoordinates Coordinates { get; }

        IEventScheduler EventScheduler { get; }
    }
}
