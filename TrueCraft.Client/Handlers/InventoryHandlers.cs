using System;
using TrueCraft.Core;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Windows;

namespace TrueCraft.Client.Handlers
{
    internal static class InventoryHandlers
    {
        public static void HandleWindowItems(IPacket _packet, MultiplayerClient client)
        {
            var packet = (WindowItemsPacket)_packet;
            if (packet.WindowID == 0)
                client.InventoryWindow.SetSlots(packet.Items);
            else
                client.CurrentWindow.SetSlots(packet.Items);
        }

        public static void HandleSetSlot(IPacket _packet, MultiplayerClient client)
        {
            var packet = (SetSlotPacket)_packet;
            IWindow<ISlot> window = null;
            if (packet.WindowID == 0)
                window = client.InventoryWindow;
            else
                window = client.CurrentWindow;
            if (window != null)
            {
                if (packet.SlotIndex >= 0 && packet.SlotIndex < window.Count)
                {
                    window[packet.SlotIndex] = new ItemStack(packet.ItemID, packet.Count, packet.Metadata);
                }
            }
        }

        public static void HandleOpenWindowPacket(IPacket _packet, MultiplayerClient client)
        {
            var packet = (OpenWindowPacket)_packet;
            sbyte windowID = packet.WindowID;
            IInventoryFactory<ISlot> factory = new InventoryFactory<ISlot>();
            IItemRepository itemRepository = ItemRepository.Get();
            ISlotFactory<ISlot> slotFactory = SlotFactory<ISlot>.Get();

            IWindow<ISlot> window = null;
            switch (packet.Type)
            {
                case WindowType.CraftingBench:
                    window = factory.NewCraftingBenchWindow(itemRepository,
                        CraftingRepository.Get(), slotFactory,
                        windowID, client.Inventory, client.Hotbar, packet.Title, 3, 3);    // TODO hard-coded constants
                    break;

                case WindowType.Chest:
                    window = factory.NewChestWindow(itemRepository, slotFactory,
                        windowID, client.Inventory, client.Hotbar,
                        null, null, null);
                    break;
            }

            // TODO: For any window type other than CraftingBench or Chest, window will be null.
            client.CurrentWindow = window;
        }

        public static void HandleCloseWindowPacket(IPacket _packet, MultiplayerClient client)
        {
            client.CurrentWindow = null;
        }

        public static void HandleTransactionStatusPacket(IPacket packet, MultiplayerClient client)
        {
            TransactionStatusPacket statusPacket = (TransactionStatusPacket)packet;
            ActionConfirmation action = ActionList.Get(statusPacket.ActionNumber);

            if (object.ReferenceEquals(action, null))
                throw new ApplicationException($"Unexpected Action Number from server: {statusPacket.ActionNumber}");

            action.TakeAction();
        }
    }
}