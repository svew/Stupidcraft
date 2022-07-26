using System;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.World;
using TrueCraft.Core.Physics;
using TrueCraft.Core.Server;

namespace TrueCraft.Core.Entities
{
    public abstract class FallingBlockEntity : ObjectEntity
    {
        public FallingBlockEntity(IDimension dimension, IEntityManager entityManager,
            Vector3 position) :
            base(dimension, entityManager, new Size(0.98), 0.8f, 0.40f, 39.2f)
        {
            _position = position + new Vector3(0.5);
        }

        public override IPacket SpawnPacket
        {
            get
            {
                return new SpawnGenericEntityPacket(EntityID, (sbyte)EntityType,
                    MathHelper.CreateAbsoluteInt(Position.X), MathHelper.CreateAbsoluteInt(Position.Y),
                    MathHelper.CreateAbsoluteInt(Position.Z));
            }
        }

        public override int Data { get { return 1; } }
    }
}