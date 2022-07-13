using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using TrueCraft.Core;
using System.Threading;
using System.Reflection;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Inventory;
using TrueCraft.Client.Handlers;

namespace TrueCraft.Client
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            WhoAmI.Answer = IAm.Client;
            TrueCraft.Core.Inventory.InventoryFactory<ISlot>.RegisterInventoryFactory(new TrueCraft.Client.Inventory.InventoryFactory());
            TrueCraft.Core.Inventory.SlotFactory<ISlot>.RegisterSlotFactory(new TrueCraft.Client.Inventory.SlotFactory());

            UserSettings.Local.Load();

            IServiceLocator serviceLocator = Discover.DoDiscovery(new Discover());
            InventoryHandlers.ItemRepository = serviceLocator.ItemRepository;

            IPEndPoint? serverEndPoint = null;

            try
            {
                serverEndPoint = ParseEndPoint(args[0]);
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return;
            }
            if (serverEndPoint is null)
            {
                Console.Error.WriteLine($"Unable to resolve server: {args[0]}");
                return;
            }

            var user = new TrueCraftUser { Username = args[1] };
            MultiplayerClient client = new MultiplayerClient(serviceLocator, user);
            TrueCraftGame game = new TrueCraftGame(serviceLocator, client, serverEndPoint);
            game.Run();
            client.Disconnect();
        }

        private static IPEndPoint? ParseEndPoint(string arg)
        {
            IPAddress? address;
            int port;

            if (arg.Contains(':'))
            {
                // Both IP and port are specified
                var parts = arg.Split(':');
                if (!IPAddress.TryParse(parts[0], out address))
                    address = Resolve(parts[0]);
                if (address is null)
                    return null;
                return new IPEndPoint(address, int.Parse(parts[1]));
            }

            if (IPAddress.TryParse(arg, out address))
                return new IPEndPoint(address, 25565);

            if (int.TryParse(arg, out port))
                return new IPEndPoint(IPAddress.Loopback, port);

            address = Resolve(arg);
            if (address is not null)
                return new IPEndPoint(address, 25565);
            else
                return null;
        }

        private static IPAddress? Resolve(string arg)
        {
            return Dns.GetHostEntry(arg).AddressList.FirstOrDefault(item => item.AddressFamily == AddressFamily.InterNetwork);
        }
    }
}
