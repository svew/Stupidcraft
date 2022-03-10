using System;
using TrueCraft.Core.Logic.Items;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;
using TrueCraft.Core.Server;
using TrueCraft.Core.Entities;

namespace TrueCraft.Core.Logic.Blocks
{
    public class GravelBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x0D;
        
        public override byte ID { get { return 0x0D; } }
        
        public override double BlastResistance { get { return 3; } }

        public override double Hardness { get { return 0.6; } }

        public override byte Luminance { get { return 0; } }
        
        public override string GetDisplayName(short metadata)
        {
            return "Gravel";
        }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Gravel;
            }
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(3, 1);
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            //Gravel has a 10% chance of dropping flint.
            if (MathHelper.Random.Next(10) == 0)
                return new[] { new ItemStack(FlintItem.ItemID, 1, descriptor.Metadata) };
            else
                return new ItemStack[0];
        }

        public override void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IDimension world, IRemoteClient user)
        {
            BlockUpdate(descriptor, descriptor, user.Server, world);
        }

        public override void BlockUpdate(BlockDescriptor descriptor, BlockDescriptor source, IMultiplayerServer server, IDimension world)
        {
            if (world.GetBlockID(descriptor.Coordinates + Vector3i.Down) == AirBlock.BlockID)
            {
                world.SetBlockID(descriptor.Coordinates, AirBlock.BlockID);
                server.GetEntityManagerForWorld(world).SpawnEntity(new FallingGravelEntity((Vector3)descriptor.Coordinates));
            }
        }
    }
}