using System;
using System.Linq;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Server;
using TrueCraft.Core.Physics;
using System.Collections.Generic;

namespace TrueCraft.Core.Entities
{
    public class ItemEntity : ObjectEntity, IAABBEntity
    {
        public static float PickupRange = 2;

        public ItemEntity(Vector3 position, ItemStack item)
        {
            Position = position;
            Item = item;
            Velocity = new Vector3(MathHelper.Random.NextDouble() * 0.25 - 0.125, 0.25, MathHelper.Random.NextDouble() * 0.25 - 0.125);
            if (item.Empty)
                Despawned = true;
        }

        public ItemStack Item { get; set; }

        public override IPacket SpawnPacket
        {
            get
            {
                return new SpawnItemPacket(EntityID, Item.ID, Item.Count, Item.Metadata,
                    MathHelper.CreateAbsoluteInt(Position.X), MathHelper.CreateAbsoluteInt(Position.Y),
                    MathHelper.CreateAbsoluteInt(Position.Z),
                    MathHelper.CreateRotationByte(Yaw),
                    MathHelper.CreateRotationByte(Pitch), 0);
            }
        }

        public override Size Size
        {
            get { return new Size(0.25, 0.25, 0.25); }
        }

        public virtual BoundingBox BoundingBox
        {
            get
            {
                return new BoundingBox(Position, Position + Size);
            }
        }

        public void TerrainCollision(Vector3 collisionPoint, Vector3 collisionDirection)
        {
            if (collisionDirection == Vector3.Down)
            {
                Velocity = Vector3.Zero;
            }
        }

        public override byte EntityType
        {
            get { return 2; }
        }

        public override int Data
        {
            get { return 1; }
        }

        public override MetadataDictionary Metadata
        {
            get
            {
                var metadata = base.Metadata;
                metadata[10] = Item;
                return metadata;
            }
        }

        public override bool SendMetadataToClients
        {
            get
            {
                return false;
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

        public override void Update(IEntityManager entityManager)
        {
            IList<IEntity> nearbyEntities = entityManager.EntitiesInRange(Position, PickupRange);
            if ((DateTime.UtcNow - SpawnTime).TotalSeconds > 1)
            {
                IEntity player = nearbyEntities.FirstOrDefault(e => e is PlayerEntity && ((PlayerEntity)e).Health != 0
                    && e.Position.DistanceTo(Position) <= PickupRange);
                if (player is not null)
                {
                    PlayerEntity playerEntity = (PlayerEntity)player;
                    playerEntity.OnPickUpItem(this);
                    // TODO BUG: what if the player only has room for a partial pickup?
                    entityManager.DespawnEntity(this);
                }
            }
            if ((DateTime.UtcNow - SpawnTime).TotalMinutes > 5)
                entityManager.DespawnEntity(this);
            base.Update(entityManager);
        }

        public float AccelerationDueToGravity
        {
            get { return 1.98f; }
        }

        public float Drag
        {
            get { return 0.40f; }
        }

        public float TerminalVelocity
        {
            get { return 39.2f; }
        }
    }
}
