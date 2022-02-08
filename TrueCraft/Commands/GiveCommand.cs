using System;
using System.Collections.Generic;
using System.Linq;
using TrueCraft.Core;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Server;
using TrueCraft.Core.Windows;

namespace TrueCraft.Commands
{
    public class GiveCommand : Command
    {
        public override string Name
        {
            get { return "give"; }
        }

        public override string Description
        {
            get { return "Give the specified player an amount of items."; }
        }

        public override string[] Aliases
        {
            get { return new string[1]{ "i" }; }
        }

        public override void Handle(IRemoteClient client, string alias, string[] arguments)
        {
            if (arguments.Length < 2)
            {
                Help(client, alias, arguments);
                return;
            }

            string  username    = arguments[0],
                    itemid      = arguments[1],
                    amount      = "1";

            if(arguments.Length >= 3)
                    amount = arguments[2];
            
            var receivingPlayer = GetPlayerByName(client, username);

            if (receivingPlayer == null)
            {
                client.SendMessage("No client with the username \"" + username + "\" was found.");
                return;
            }

            if (!GiveItem(receivingPlayer, itemid, amount, client))
            {
                Help(client, alias, arguments);
            }
        }

        protected static IRemoteClient GetPlayerByName(IRemoteClient client, string username)
        {
            var receivingPlayer =
                client.Server.Clients.FirstOrDefault(
                    c => String.Equals(c.Username, username, StringComparison.CurrentCultureIgnoreCase));
            return receivingPlayer;
        }

        protected static bool GiveItem(IRemoteClient receivingPlayer, string itemid, string amount, IRemoteClient client)
        {
            short id;
            short metadata = 0;
            int count;

            if (itemid.Contains(":"))
            {
                var parts = itemid.Split(':');
                if (!short.TryParse(parts[0], out id) || !short.TryParse(parts[1], out metadata) || !Int32.TryParse(amount, out count)) return false;
            }
            else
            {
                if (!short.TryParse(itemid, out id) || !Int32.TryParse(amount, out count)) return false;
            }

            if (client.Server.ItemRepository.GetItemProvider(id) == null)
            {
                client.SendMessage("Invalid item id \"" + id + "\".");
                return true;
            }

            string username = receivingPlayer.Username;
            IInventoryWindow<IServerSlot> inventory = receivingPlayer.InventoryWindowContent;
            if (inventory == null) return false;

            List<int> affectedSlotIndices;
            List<int> slotIndicesToSend = new List<int>();
            List<ItemStack> newContents = null;
            List<ItemStack> slotContentToSend = new List<ItemStack>();

            while (count > 0)
            {
                sbyte amountToGive;
                if (count >= 64)
                    amountToGive = 64;
                else
                    amountToGive = (sbyte) count;

                count -= amountToGive;

                inventory.StoreItemStack(new ItemStack(id, amountToGive, metadata), out affectedSlotIndices, out newContents);

                // Accumulate only the last version of each affected slot.
                for (int j = 0; j < affectedSlotIndices.Count; j ++)
                {
                    int k = 0;
                    while (k < slotIndicesToSend.Count && affectedSlotIndices[j] != slotIndicesToSend[k])
                        k++;
                    if (k == slotIndicesToSend.Count)
                    {
                        slotIndicesToSend.Add(affectedSlotIndices[j]);
                        slotContentToSend.Add(newContents[j]);
                    }
                    else
                    {
                        slotContentToSend[k] = newContents[j];
                    }
                }
            }

            for (int j = 0; j < slotIndicesToSend.Count; j ++)
            {
                ItemStack item = newContents[j];
                receivingPlayer.QueuePacket(
                    new SetSlotPacket(receivingPlayer.InventoryWindowContent.WindowID,
                    (short)slotIndicesToSend[j], item.ID, item.Count, item.Metadata));
            }

            // TODO If the player's active hotbar slot has been altered, send the appropriate packets.

            return true;
        }

        public override void Help(IRemoteClient client, string alias, string[] arguments)
        {
            client.SendMessage("Correct usage is /" + alias + " <User> <Item ID> [Amount]");
        }
    }
}
