﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrueCraft.Core;
using TrueCraft.Core.AI;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Lighting;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Commands
{
    public class PositionCommand : Command
    {
        public override string Name
        {
            get { return "pos"; }
        }

        public override string Description
        {
            get { return "Shows your position."; }
        }

        public override string[] Aliases
        {
            get { return new string[0]; }
        }

        public override void Handle(IRemoteClient client, string alias, string[] arguments)
        {
            if (arguments.Length != 0)
            {
                Help(client, alias, arguments);
                return;
            }
            client.SendMessage(client.Entity!.Position.ToString());
        }

        public override void Help(IRemoteClient client, string alias, string[] arguments)
        {
            client.SendMessage("/pos: Shows your position.");
        }
    }

    public class SaveCommand : Command
    {
        public override string Name
        {
            get { return "save"; }
        }

        public override string Description
        {
            get { return "Saves the dimension!"; }
        }

        public override string[] Aliases
        {
            get { return new string[0]; }
        }

        public override void Handle(IRemoteClient client, string alias, string[] arguments)
        {
            if (arguments.Length != 0)
            {
                Help(client, alias, arguments);
                return;
            }
            ((IDimensionServer)client.Dimension!).Save();
        }

        public override void Help(IRemoteClient client, string alias, string[] arguments)
        {
            client.SendMessage("/save: Saves the dimension!");
        }
    }

    public class SkyLightCommand : Command
    {
        public override string Name
        {
            get { return "sl"; }
        }

        public override string Description
        {
            get { return "Shows sky light at your current position."; }
        }

        public override string[] Aliases
        {
            get { return new string[0]; }
        }

        public override void Handle(IRemoteClient client, string alias, string[] arguments)
        {
            int mod = 0;
            if (arguments.Length == 1)
                int.TryParse(arguments[0], out mod);
            client.SendMessage(client.Dimension!.GetSkyLight(
                (GlobalVoxelCoordinates)(client.Entity!.Position + new Vector3(0, -mod, 0))).ToString());
        }

        public override void Help(IRemoteClient client, string alias, string[] arguments)
        {
            client.SendMessage("/sl");
        }
    }

    public class SpawnCommand : Command
    {
        public override string Name
        {
            get { return "spawn"; }
        }

        public override string Description
        {
            get { return "Spawns a mob."; }
        }

        public override string[] Aliases
        {
            get { return new string[0]; }
        }

        public override void Handle(IRemoteClient client, string alias, string[] arguments)
        {
            if (arguments.Length != 1)
            {
                Help(client, alias, arguments);
                return;
            }
            var entityTypes = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in assembly.GetTypes())
                {
                    if (typeof(IEntity).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                        entityTypes.Add(t);
                }
            }

            arguments[0] = arguments[0].ToUpper();
            Type? type = entityTypes.SingleOrDefault(t => t.Name.ToUpper() == arguments[0] + "ENTITY");
            if (type is null)
            {
                client.SendMessage(ChatColor.Red + "Unknown entity type.");
                return;
            }

            IEntity? entity = (IEntity?)Activator.CreateInstance(type);
            if (entity is null)
            {
                client.SendMessage(ChatColor.Red + "Unable to create entity.  Most likely a missing default constructor.");
                return;
            }

            IEntityManager em = ((IDimensionServer)client.Dimension!).EntityManager;
            entity.Position = client.Entity!.Position + MathHelper.FowardVector(client.Entity.Yaw) * 3;
            em.SpawnEntity(entity);
        }

        public override void Help(IRemoteClient client, string alias, string[] arguments)
        {
            client.SendMessage("/spawn [type]: Spawns a mob of that type.");
        }
    }

    public class ToMeCommand : Command
    {
        public override string Name
        {
            get { return "tome"; }
        }

        public override string Description
        {
            get { return "Moves a mob towards your position."; }
        }

        public override string[] Aliases
        {
            get { return new string[0]; }
        }

        public override void Handle(IRemoteClient client, string alias, string[] arguments)
        {
            if (arguments.Length != 1)
            {
                Help(client, alias, arguments);
                return;
            }

            int id;
            if (!int.TryParse(arguments[0], out id))
            {
                Help(client, alias, arguments);
                return;
            }

            IEntityManager manager = ((IDimensionServer)client.Dimension!).EntityManager;
            var entity = manager.GetEntityByID(id) as MobEntity;
            if (entity == null)
            {
                client.SendMessage(ChatColor.Red + "An entity with that ID does not exist in this dimension.");
                return;
            }

            Task.Factory.StartNew(() =>
            {
                var astar = new AStarPathFinder();
                PathResult? path = astar.FindPath(client.Dimension, entity.BoundingBox, (GlobalVoxelCoordinates)entity.Position, (GlobalVoxelCoordinates)client.Entity!.Position);
                if (path is null)
                {
                    client.SendMessage(ChatColor.Red + "It is impossible for this entity to reach you.");
                }
                else
                {
                    client.SendMessage(string.Format(ChatColor.Blue
                        + "Executing path with {0} waypoints", path.Count));
                    entity.CurrentPath = path;
                }
            });
        }

        public override void Help(IRemoteClient client, string alias, string[] arguments)
        {
            client.SendMessage("/tome [id]: Moves a mob to your position.");
        }
    }

    public class EntityInfoCommand : Command
    {
        public override string Name
        {
            get { return "entity"; }
        }

        public override string Description
        {
            get { return "Provides information about an entity ID."; }
        }

        public override string[] Aliases
        {
            get { return new string[0]; }
        }

        public override void Handle(IRemoteClient client, string alias, string[] arguments)
        {
            if (arguments.Length != 1)
            {
                Help(client, alias, arguments);
                return;
            }

            int id;
            if (!int.TryParse(arguments[0], out id))
            {
                Help(client, alias, arguments);
                return;
            }

            IEntityManager manager = ((IDimensionServer)client.Dimension!).EntityManager;
            var entity = manager.GetEntityByID(id);
            if (entity is null)
            {
                client.SendMessage(ChatColor.Red + "An entity with that ID does not exist in this dimension.");
                return;
            }
            client.SendMessage(string.Format(
                "{0} {1}", entity.GetType().Name, entity.Position));
            if (entity is MobEntity)
            {
                MobEntity mob = (MobEntity)entity;
                client.SendMessage(string.Format(
                    "{0}/{1} HP, {2} State, moving to to {3}",
                    mob.Health, mob.MaxHealth,
                    mob.CurrentState?.GetType().Name ?? "null",
                    mob.CurrentPath?.Last().ToString() ?? "null"));
            }
        }

        public override void Help(IRemoteClient client, string alias, string[] arguments)
        {
            client.SendMessage("/entity [id]: Shows information about this entity.");
        }
    }

    public class DestroyCommand : Command
    {
        public override string Name
        {
            get { return "destroy"; }
        }

        public override string Description
        {
            get { return "Destroys a mob. Violently."; }
        }

        public override string[] Aliases
        {
            get { return new string[0]; }
        }

        public override void Handle(IRemoteClient client, string alias, string[] arguments)
        {
            if (arguments.Length != 1)
            {
                Help(client, alias, arguments);
                return;
            }

            int id;
            if (!int.TryParse(arguments[0], out id))
            {
                Help(client, alias, arguments);
                return;
            }

            IEntityManager manager = ((IDimensionServer)client.Dimension!).EntityManager;
            MobEntity? entity = manager.GetEntityByID(id) as MobEntity;
            if (entity is null)
            {
                client.SendMessage(ChatColor.Red + "An entity with that ID does not exist in this world.");
                return;
            }

            manager.DespawnEntity(entity);
        }

        public override void Help(IRemoteClient client, string alias, string[] arguments)
        {
            client.SendMessage("/destroy [id]: " + Description);
        }
    }

    public class TrashCommand : Command
    {
        public override string Name
        {
            get { return "trash"; }
        }

        public override string Description
        {
            get { return "Discards selected item, hotbar, or entire inventory."; }
        }

        public override string[] Aliases
        {
            get { return new string[0]; }
        }

        public override void Handle(IRemoteClient client, string alias, string[] arguments)
        {
            if (arguments.Length != 0)
            {
                if (arguments[0] == "hotbar")
                {
                    // Discard hotbar items
                    for (short i = 0; i <client.Hotbar.Count; i++)
                        client.Hotbar[i].Item = ItemStack.EmptyStack;
                }
                else if (arguments[0] == "all")
                {
                    // Discard all inventory items, including armor and crafting area contents
                    for (int i = 0; i < client.Hotbar.Count; i++)
                        client.Hotbar[i].Item = ItemStack.EmptyStack;
                    for (int i = 0; i < client.Inventory.Count; i++)
                        client.Inventory[i].Item = ItemStack.EmptyStack;
                    for (int i = 0; i < client.Armor.Count; i++)
                        client.Armor[i].Item = ItemStack.EmptyStack;
                    for (int i = 0; i < client.CraftingGrid.Count; i++)
                        client.CraftingGrid[i].Item = ItemStack.EmptyStack;
                }
                else
                {
                    Help(client, alias, arguments);
                    return;
                }
            }
            else
            {
                // Discards selected item.
                client.Hotbar[client.SelectedSlot].Item = ItemStack.EmptyStack;
            }
        }

        public override void Help(IRemoteClient client, string alias, string[] arguments)
        {
            client.SendMessage("Correct usage is /trash <hotbar/all> or leave blank to clear\nselected slot.");
        }
    }

    public class WhatCommand : Command
    {
        public override string Name
        {
            get { return "what"; }
        }

        public override string Description
        {
            get { return "Tells you what you're holding."; }
        }

        public override string[] Aliases
        {
            get { return new string[0]; }
        }

        public override void Handle(IRemoteClient client, string alias, string[] arguments)
        {
            if (arguments.Length != 0)
            {
                Help(client, alias, arguments);
                return;
            }
            client.SendMessage(client.SelectedItem.ToString());
        }

        public override void Help(IRemoteClient client, string alias, string[] arguments)
        {
            client.SendMessage("/what: Tells you what you're holding.");
        }
    }

    public class LogCommand : Command
    {
        public override string Name
        {
            get { return "log"; }
        }

        public override string Description
        {
            get { return "Toggles client logging."; }
        }

        public override string[] Aliases
        {
            get { return new string[0]; }
        }

        public override void Handle(IRemoteClient client, string alias, string[] arguments)
        {
            if (arguments.Length != 0)
            {
                Help(client, alias, arguments);
                return;
            }
            client.EnableLogging = !client.EnableLogging;
        }

        public override void Help(IRemoteClient client, string alias, string[] arguments)
        {
            client.SendMessage("/log: Toggles client logging.");
        }
    }
    
    public class ResendInvCommand : Command
    {
        public override string Name
        {
            get { return "reinv"; }
        }

        public override string Description
        {
            get { return "Resends your inventory to the selected client."; }
        }

        public override string[] Aliases
        {
            get { return new string[0]; }
        }

        public override void Handle(IRemoteClient client, string alias, string[] arguments)
        {
            if (arguments.Length != 0)
            {
                Help(client, alias, arguments);
                return;
            }
            // TODO: The original behaviour was to resend all 4 sub-areas of the
            //   Player's main inventory window:
            //      - Armor
            //      - 2x2 crafting grid
            //      - player's inventory
            //      - hotbar.
            // This now sends just the Player's Inventory and hotbar.
            // It is not clear which is correct.
            ItemStack[] items = new ItemStack[client.Inventory.Count + client.Hotbar.Count];
            int idx = 0;
            int j;
            for (j = 0; j < client.Inventory.Count; j++, idx ++)
                items[idx] = client.Inventory[j].Item;
            for (j = 0; j < client.Hotbar.Count; j++, idx++)
                items[idx] = client.Hotbar[j].Item;
            client.QueuePacket(new WindowItemsPacket(0, items));
        }

        public override void Help(IRemoteClient client, string alias, string[] arguments)
        {
            client.SendMessage("/reinv: Resends your inventory.");
        }
    }

    public class RelightCommand : Command
    {
        public override string Name
        {
            get { return "relight"; }
        }

        public override string Description
        {
            get { return "Relights the chunk you're standing in."; }
        }

        public override string[] Aliases
        {
            get { return new string[0]; }
        }

        public override void Handle(IRemoteClient client, string alias, string[] arguments)
        {
            if (arguments.Length != 0)
            {
                Help(client, alias, arguments);
                return;
            }
            MultiplayerServer server = (MultiplayerServer)client.Server;
            IChunk chunk = client.Dimension!.GetChunk((GlobalVoxelCoordinates)client.Entity!.Position)!;
            Lighting? lighter = server.WorldLighters.SingleOrDefault(l => l.Dimension == client.Dimension);
            if (lighter is not null)
            {
                // TODO: what does it mean to queue up initial lighting, then unload and reload the chunk?
                lighter.InitialLighting(chunk, true);
                ((RemoteClient)client).UnloadChunk(chunk.Coordinates);
                ((RemoteClient)client).LoadChunk(chunk);
            }
        }

        public override void Help(IRemoteClient client, string alias, string[] arguments)
        {
            client.SendMessage("/relight: Relights the chunk you're standing in.");
        }
    }
}