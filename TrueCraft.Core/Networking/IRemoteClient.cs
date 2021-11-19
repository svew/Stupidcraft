using System;
using TrueCraft.Core.World;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Windows;
using TrueCraft.Core.Server;

namespace TrueCraft.Core.Networking
{
    public interface IRemoteClient
    {
        /// <summary>
        /// Minecraft stream used to communicate with this client.
        /// </summary>
        IMinecraftStream MinecraftStream { get; }
        /// <summary>
        /// Returns true if this client has data pending in the network stream.
        /// </summary>
        bool DataAvailable { get; }
        /// <summary>
        /// The world this client is present in.
        /// </summary>
        IWorld World { get; }
        /// <summary>
        /// The entity associated with this client.
        /// </summary>
        IEntity Entity { get; }

        /// <summary>
        /// This client's main inventory.
        /// </summary>
        ISlots Inventory { get; }

        /// <summary>
        /// This client's HotBar slots.
        /// </summary>
        ISlots Hotbar { get; }

        /// <summary>
        /// This client's Armor slots.
        /// </summary>
        ISlots Armor { get; }

        /// <summary>
        /// Gets this client's Crafting Grid.
        /// </summary>
        ISlots CraftingGrid { get; }

        /// <summary>
        /// Gets the IWindowContent representing the player's inventory, hotbar, crafting grid, and armor.
        /// </summary>
        IInventoryWindowContent InventoryWindowContent { get; }

        /// <summary>
        /// The username of the connected client. May be null if not yet ascertained.
        /// </summary>
        string Username { get; }
        /// <summary>
        /// The slot index this user has selected in their hotbar.
        /// </summary>
        short SelectedSlot { get; }
        /// <summary>
        /// The item stack at the slot the user has selected in their hotbar.
        /// </summary>
        ItemStack SelectedItem { get; }
        /// <summary>
        /// The server this user is playing on.
        /// </summary>
        IMultiplayerServer Server { get; }
        /// <summary>
        /// If true, this client will be sent logging information as chat messages.
        /// </summary>
        bool EnableLogging { get; set; }
        /// <summary>
        /// The time the user is expected to complete the active digging operation,
        /// depending on what kind of block they are mining and what tool they're using
        /// to do it with.
        /// </summary>
        DateTime ExpectedDigComplete { get; set; }
        /// <summary>
        /// True if this client has been disconnected. You should cease sending packets and
        /// so on, this client is just waiting to be reaped.
        /// </summary>
        bool Disconnected { get; }

        /// <summary>
        /// Loads player data from disk for this client.
        /// </summary>
        bool Load();
        /// <summary>
        /// Saves player data to disk for this client.
        /// </summary>
        void Save();
        /// <summary>
        /// Queues a packet to be sent to this client.
        /// </summary>
        void QueuePacket(IPacket packet);
        /// <summary>
        /// Disconnects this client from the server.
        /// </summary>
        void Disconnect();
        /// <summary>
        /// Sends a chat message to this client.
        /// </summary>
        void SendMessage(string message);
        /// <summary>
        /// If logging is enabled, sends your message to the client as chat.
        /// </summary>
        void Log(string message, params object[] parameters);
        /// <summary>
        /// Opens a window on the client. This sends the appropriate packets and tracks
        /// this window as the currently open window.
        /// </summary>
        void OpenWindow(IWindowContent window);
    }
}
