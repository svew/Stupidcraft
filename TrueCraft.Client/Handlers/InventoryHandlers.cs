using System;
using TrueCraft.Client.Inventory;
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
        private static IItemRepository _itemRepository = null!;

        public static IItemRepository ItemRepository
        {
            set
            {
                _itemRepository = value;
            }
        }

        public static void HandleWindowItems(IPacket _packet, MultiplayerClient client)
        {
            var packet = (WindowItemsPacket)_packet;
            if (packet.WindowID == 0)
                client.InventoryWindow.SetSlots(packet.Items);
            else
                client.CurrentWindow?.SetSlots(packet.Items);
        }

        public static void HandleSetSlot(IPacket _packet, MultiplayerClient client)
        {
            var packet = (SetSlotPacket)_packet;
            IWindow<ISlot>? window = null;
            if (packet.WindowID == 0)
                window = client.InventoryWindow;
            else
                window = client.CurrentWindow;
            if (window is not null && packet.SlotIndex >= 0 && packet.SlotIndex < window.Count)
                window[packet.SlotIndex] = new ItemStack(packet.ItemID, packet.Count, packet.Metadata);
        }

        public static void HandleOpenWindowPacket(IPacket _packet, MultiplayerClient client)
        {
            var packet = (OpenWindowPacket)_packet;
            sbyte windowID = packet.WindowID;
            ISlotFactory<ISlot> slotFactory = SlotFactory<ISlot>.Get();

            // NOTE: Since we are instantiating client implementations of the
            //       windows from within the Client, there is no need to invoke the
            //       Inventory Factory.
            IWindow<ISlot> window;
            switch (packet.Type)
            {
                case WindowType.CraftingBench:
                    window = new CraftingBenchWindow(_itemRepository, CraftingRepository.Get(),
                        slotFactory,  windowID, client.Inventory, client.Hotbar, packet.Title, 3, 3);    // TODO hard-coded constants
                    break;

                case WindowType.Chest:
                    window = new ChestWindow(_itemRepository, slotFactory, windowID,
                        client.Inventory, client.Hotbar, packet.TotalSlots == 2 * ChestWindow.ChestLength);
                    break;

                case WindowType.Furnace:
                    window = new FurnaceWindow(_itemRepository, slotFactory, windowID,
                        client.Inventory, client.Hotbar);
                    break;

                default:
                    throw new ApplicationException($"Unknown Window Type: {packet.Type}");
            }

            client.CurrentWindow = window;
        }

        public static void HandleUpdateProgressPacket(IPacket packet, MultiplayerClient client)
        {
            IFurnaceProgress? furnace = client.CurrentWindow as IFurnaceProgress;
            if (furnace is null)
                return;

            UpdateProgressPacket progressPacket = (UpdateProgressPacket)packet;

            if (progressPacket.Target == UpdateProgressPacket.ProgressTarget.ItemCompletion)
                furnace.SmeltingProgress = progressPacket.Value;
            else if (progressPacket.Target == UpdateProgressPacket.ProgressTarget.AvailableHeat)
                furnace.BurnProgress = progressPacket.Value;
        }

        public static void HandleCloseWindowPacket(IPacket _packet, MultiplayerClient client)
        {
            client.CurrentWindow = null;
        }

        public static void HandleTransactionStatusPacket(IPacket packet, MultiplayerClient client)
        {
            TransactionStatusPacket statusPacket = (TransactionStatusPacket)packet;
            ActionConfirmation? action = ActionList.Get(statusPacket.ActionNumber);

            if (action is null)
                throw new ApplicationException($"Unexpected Action Number from server: {statusPacket.ActionNumber}");

            action.TakeAction();
        }
    }
}