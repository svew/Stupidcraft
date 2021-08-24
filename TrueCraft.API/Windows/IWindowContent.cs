using System;
using TrueCraft.API.Networking;

namespace TrueCraft.API.Windows
{
    /// <summary>
    /// An enum to specify types of child windows.
    /// </summary>
    /// <remarks>These values are the ones used in the Type field of the OpenWindowPacket.</remarks>
    public enum WindowType : sbyte
    {
        Inventory = -1,
        Chest = 0,
        CraftingBench = 1,
        Furnace = 2,
        Dispenser = 3
    }

    public interface IWindowContent : IDisposable, IEventSubject
    {
        event EventHandler<WindowChangeEventArgs> WindowChange;

        IRemoteClient Client { get; set; }

        sbyte ID { get; set; }
        string Name { get; }

        /// <summary>
        /// Gets the type of the Window for which this stores the content.
        /// </summary>
        WindowType Type { get; }

        int Length { get; }
        int Length2 { get; }
        ItemStack this[int index] { get; set; }

        short[] ReadOnlySlots { get; }

        /// <summary>
        /// Gets an array of all slots in this window. Suitable for sending to clients over the network.
        /// </summary>
        ItemStack[] GetSlots();

        void SetSlots(ItemStack[] slots);

        /// <summary>
        /// Moves as many as possible of the specified ItemStack to another area of the Window Content.
        /// </summary>
        /// <param name="index">The index within the overall Window Content from
        /// which we are to move the ItemStack</param>
        /// <returns>
        /// An ItemStack containing any leftover items that would not fit.
        /// If all given Items fit in these Slots, then ItemStack.EmptyStack will
        /// be returned.
        /// </returns>
        ItemStack MoveItemStack(int index);
    }
}
