using System;

namespace TrueCraft.Core.Networking.Packets
{
    /// <summary>
    /// Spawns entities that don't fit into any other bucket of entities.
    /// </summary>
    public struct SpawnGenericEntityPacket : IPacket
    {
        public byte ID { get { return 0x17; } }

        /// <summary>
        /// Constructs a SpawnGenericEntityPacket with the optional fields zeroed.
        /// </summary>
        /// <param name="entityID">The Entity ID to spawn.</param>
        /// <param name="entityType">The type of the Entity</param>
        /// <param name="x">The Absolute Integer x-coordinate of the Entity.</param>
        /// <param name="y">The Absolute Integer y-coordinate of the Entity.</param>
        /// <param name="z">The Absolute Integer z-coordinate of the Entity.</param>
        public SpawnGenericEntityPacket(int entityID, sbyte entityType, int x, int y, int z)
        {
            EntityID = entityID;
            EntityType = entityType;
            X = x;
            Y = y;
            Z = z;
            Data = 0;
            XVelocity = 0;
            YVelocity = 0;
            ZVelocity = 0;
        }

        public int EntityID;
        public sbyte EntityType; // TODO: Enum? Maybe a lookup would be better.
        public int X, Y, Z;
        public int Data;
        public short XVelocity, YVelocity, ZVelocity;

        public void ReadPacket(IMinecraftStream stream)
        {
            EntityID = stream.ReadInt32();
            EntityType = stream.ReadInt8();
            X = stream.ReadInt32();
            Y = stream.ReadInt32();
            Z = stream.ReadInt32();
            Data = stream.ReadInt32();
            if (Data > 0)
            {
                XVelocity = stream.ReadInt16();
                YVelocity = stream.ReadInt16();
                ZVelocity = stream.ReadInt16();
            }
            else
            {
                XVelocity = 0;
                YVelocity = 0;
                ZVelocity = 0;
            }
        }

        public void WritePacket(IMinecraftStream stream)
        {
            stream.WriteInt32(EntityID);
            stream.WriteInt8(EntityType);
            stream.WriteInt32(X);
            stream.WriteInt32(Y);
            stream.WriteInt32(Z);
            stream.WriteInt32(Data);
            if (Data > 0)
            {
                stream.WriteInt16(XVelocity);
                stream.WriteInt16(YVelocity);
                stream.WriteInt16(ZVelocity);
            }
        }
    }
}