using System;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.World;
using TrueCraft.Core.Physics;
using TrueCraft.Core.Server;

namespace TrueCraft.Core.Entities
{
    public abstract class FallingBlockEntity : ObjectEntity, IAABBEntity
    {
        public FallingBlockEntity(IDimension dimension, IEntityManager entityManager,
            Vector3 position) : base(dimension, entityManager)
        {
            _Position = position + new Vector3(0.5);
        }

        public override Size Size
        {
            get
            {
                return new Size(0.98);
            }
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

        public abstract void TerrainCollision(Vector3 collisionPoint, Vector3 collisionDirection);

        public BoundingBox BoundingBox
        {
            get
            {
                return new BoundingBox(Position - (Size / 2), Position + (Size / 2));
            }
        }

        public bool BeginUpdate()
        {
            EnablePropertyChange = false;
            return true;
        }

        public void EndUpdate(Vector3 newPosition)
        {
            EnablePropertyChange = true;
            Position = newPosition;
        }

        public float AccelerationDueToGravity
        {
            get
            {
                return 0.8f;
            }
        }

        public float Drag
        {
            get
            {
                return 0.40f;
            }
        }

        public float TerminalVelocity
        {
            get
            {
                return 39.2f;
            }
        }
    }
}