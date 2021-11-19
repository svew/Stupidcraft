using System;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Server;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Blocks
{
    public class SandBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x0C;
        
        public override byte ID { get { return 0x0C; } }
        
        public override double BlastResistance { get { return 2.5; } }

        public override double Hardness { get { return 0.5; } }

        public override byte Luminance { get { return 0; } }
        
        public override string DisplayName { get { return "Sand"; } }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Sand;
            }
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(2, 1);
        }

        public override void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IWorld world, IRemoteClient user)
        {
            BlockUpdate(descriptor, descriptor, user.Server, world);
        }

        public override void BlockUpdate(BlockDescriptor descriptor, BlockDescriptor source, IMultiplayerServer server, IWorld world)
        {
            if (world.GetBlockID(descriptor.Coordinates + Vector3i.Down) == AirBlock.BlockID)
            {
                world.SetBlockID(descriptor.Coordinates, AirBlock.BlockID);
                server.GetEntityManagerForWorld(world).SpawnEntity(new FallingSandEntity((Vector3)descriptor.Coordinates));
            }
        }
    }
}