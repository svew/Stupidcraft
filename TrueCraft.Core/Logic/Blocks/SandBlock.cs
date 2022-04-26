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
        
        public override string GetDisplayName(short metadata)
        {
            return "Sand";
        }

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

        public override void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IDimension dimension, IRemoteClient user)
        {
            BlockUpdate(descriptor, descriptor, user.Server, dimension);
        }

        public override void BlockUpdate(BlockDescriptor descriptor, BlockDescriptor source, IMultiplayerServer server, IDimension dimension)
        {
            ServerOnly.Assert();

            if (dimension.GetBlockID(descriptor.Coordinates + Vector3i.Down) == AirBlock.BlockID)
            {
                dimension.SetBlockID(descriptor.Coordinates, AirBlock.BlockID);
                ((IDimensionServer)server).EntityManager.SpawnEntity(new FallingSandEntity((Vector3)descriptor.Coordinates));
            }
        }
    }
}