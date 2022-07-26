using System;
using System.Linq;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Server;
using TrueCraft.Core.Physics;
using System.Collections.Generic;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public class ItemEntity : ObjectEntity
    {
        public static float PickupRange = 2;

        public ItemEntity(IDimension dimension, IEntityManager entityManager,
            Vector3 position, ItemStack item) :
            base(dimension, entityManager, new Size(0.25f),
                1.98f,   // Acceleration Due To Gravity
                0.4f,    // Drag
                39.2f)   // Terminal Velocity
        {
            Position = position;
            Item = item;
            Velocity = new Vector3(MathHelper.Random.NextDouble() * 0.25 - 0.125, 0.25, MathHelper.Random.NextDouble() * 0.25 - 0.125);
            if (item.Empty)
                Despawned = true;
        }

        public ItemStack Item { get; }

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

        public override void TerrainCollision(Vector3 collisionPoint, Vector3 collisionDirection)
        {
            if (collisionDirection == Vector3.Down)
                Velocity = Vector3.Zero;
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

        public override void Update(IEntityManager entityManager)
        {
            IList<IEntity> nearbyEntities = entityManager.EntitiesInRange(Position, PickupRange);
            if ((DateTime.UtcNow - SpawnTime).TotalSeconds > 1)
            {
                IEntity? player = nearbyEntities.FirstOrDefault(e => e is PlayerEntity && ((PlayerEntity)e).Health != 0
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
    }
}
