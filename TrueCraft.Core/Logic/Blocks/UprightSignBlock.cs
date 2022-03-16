using System;
using TrueCraft.Core.Logic.Items;
using fNbt;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Logic.Blocks
{
    public class UprightSignBlock : BlockProvider
    {
        public static readonly byte BlockID = 0x3F;
        
        public override byte ID { get { return 0x3F; } }
        
        public override double BlastResistance { get { return 5; } }

        public override double Hardness { get { return 1; } }

        public override byte Luminance { get { return 0; } }

        public override bool Opaque { get { return true; } } // This is weird. You can stack signs on signs in Minecraft.
        
        public override string GetDisplayName(short metadata)
        {
            return "Sign";
        }

        public override SoundEffectClass SoundEffect
        {
            get
            {
                return SoundEffectClass.Wood;
            }
        }

        public override BoundingBox? BoundingBox { get { return null; } }

        public override BoundingBox? InteractiveBoundingBox
        {
            get
            {
                return new BoundingBox(new Vector3(6 / 16.0, 0, 6 / 16.0), new Vector3(10 / 16.0, 10 / 16.0, 10 / 16.0));
            }
        }

        public override Tuple<int, int> GetTextureMap(byte metadata)
        {
            return new Tuple<int, int>(4, 0);
        }

        public override void BlockPlaced(BlockDescriptor descriptor, BlockFace face, IDimension dimension, IRemoteClient user)
        {
            double rotation = user.Entity.Yaw + 180 % 360;
            if (rotation < 0)
                rotation += 360;

            dimension.SetMetadata(descriptor.Coordinates, (byte)(rotation / 22.5));
        }

        protected override ItemStack[] GetDrop(BlockDescriptor descriptor, ItemStack item)
        {
            return new[] { new ItemStack(SignItem.ItemID) };
        }

        public override void BlockMined(BlockDescriptor descriptor, BlockFace face, IDimension dimension, IRemoteClient user)
        {
            dimension.SetTileEntity(descriptor.Coordinates, null);
            base.BlockMined(descriptor, face, dimension, user);
        }

        public override void TileEntityLoadedForClient(BlockDescriptor descriptor, IDimension dimension, NbtCompound entity, IRemoteClient client)
        {
            client.QueuePacket(new UpdateSignPacket
            {
                X = descriptor.Coordinates.X,
                Y = (short)descriptor.Coordinates.Y,
                Z = descriptor.Coordinates.Z,
                Text = new[]
                {
                    entity["Text1"].StringValue,
                    entity["Text2"].StringValue,
                    entity["Text3"].StringValue,
                    entity["Text4"].StringValue
                }
            });
        }
    }
}
