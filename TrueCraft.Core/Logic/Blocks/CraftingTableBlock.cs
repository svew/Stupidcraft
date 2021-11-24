using System;
using TrueCraft.Core.World;
using TrueCraft.Core.Windows;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Networking;

namespace TrueCraft.Core.Logic.Blocks
{
    public class CraftingTableBlock : BlockProvider, IBurnableItem
    {
        public static readonly byte BlockID = 0x3A;
        
        public override byte ID { get { return 0x3A; } }
        
        public override double BlastResistance { get { return 12.5; } }

        public override double Hardness { get { return 2.5; } }

        public override byte Luminance { get { return 0; } }
        
        public override string DisplayName { get { return "Crafting Table"; } }

        public TimeSpan BurnTime { get { return TimeSpan.FromSeconds(15); } }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Wood;
            }
        }

        public override bool BlockRightClicked(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user)
        {
            Server.ServerOnly.Assert();

            IWindowContentFactory factory = new WindowContentFactory();
            ICraftingBenchWindowContent window = (ICraftingBenchWindowContent)
                factory.NewCraftingBenchWindowContent(user.Inventory, user.Hotbar,
                CraftingRepository.Get(), user.Server.ItemRepository);
            user.OpenWindow(window);

            // TODO: this should be called in response to Close Window packet, not Disposed.
            window.Disposed += (sender, e) =>
            {
                // TODO BUG: this does not appear to be called (Items do not spawn, and remain in 2x2 (3x3?) Crafting Grid for next opening).
                var entityManager = user.Server.GetEntityManagerForWorld(world);
                ItemStack[] inputs = window.ClearInputs();
                foreach(ItemStack item in inputs)
                {
                    if (!item.Empty)
                    {
                        IEntity entity = new ItemEntity((Vector3)(descriptor.Coordinates + Vector3i.Up), item);
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