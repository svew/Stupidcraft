using System;
using TrueCraft.Core.World;
using TrueCraft.Core.Windows;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Server;

namespace TrueCraft.Core.Logic.Blocks
{
    public class CraftingTableBlock : BlockProvider, IBurnableItem
    {
        public static readonly byte BlockID = 0x3A;
        
        public override byte ID { get { return 0x3A; } }
        
        public override double BlastResistance { get { return 12.5; } }

        public override double Hardness { get { return 2.5; } }

        public override byte Luminance { get { return 0; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Crafting Table";
        }

        public TimeSpan BurnTime { get { return TimeSpan.FromSeconds(15); } }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Wood;
            }
        }

        public override bool BlockRightClicked(BlockDescriptor descriptor, BlockFace face, IDimension dimension, IRemoteClient user)
        {
            Server.ServerOnly.Assert();

            IInventoryFactory<IServerSlot> factory = new InventoryFactory<IServerSlot>();
            ICraftingBenchWindow<IServerSlot> window = factory.NewCraftingBenchWindow(
                ItemRepository.Get(), CraftingRepository.Get(), SlotFactory<IServerSlot>.Get(),
                WindowIDs.GetWindowID(), user.Inventory, user.Hotbar, "Crafting", 3, 3);
            user.OpenWindow(window);

            // TODO: this should be called in response to Close Window packet, not Disposed.
            window.WindowClosed += (sender, e) =>
            {
                // TODO BUG: this does not appear to be called (Items do not spawn, and remain in 2x2 (3x3?) Crafting Grid for next opening).
                IEntityManager entityManager = ((IDimensionServer)dimension).EntityManager;
                ItemStack[,] inputs = window.CraftingGrid.GetItemStacks();
                foreach(ItemStack item in inputs)
                {
                    if (!item.Empty)
                    {
                        IEntity entity = new ItemEntity(dimension, entityManager,
                            (Vector3)(descriptor.Coordinates + Vector3i.Up), item);
                        entityManager.SpawnEntity(entity);
                    }
                }
            };

            return true;
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(11, 3);
        }
    }
}