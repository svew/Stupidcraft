using System;
using System.ComponentModel;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public interface IEntity : INotifyPropertyChanged
    {
        // TODO: This is a server-side only concern.
        /// <summary>
        /// Gets a Packet that can be sent to clients to spawn this Entity
        /// </summary>
        IPacket SpawnPacket { get; }

        // TODO: Setting the Entity ID is a server-side only concern.
        /// <summary>
        /// Gets or sets the Entity ID.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that the Entity ID can only be set once.
        /// </para>
        /// </remarks>
        int EntityID { get; set; }

        /// <summary>
        /// Gets or sets the Position of the Entity.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The Position specifies the centre of the Floor of the Entity's Bounding Box.
        /// </para>
        /// </remarks>
        Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the Velocity of the Entity.
        /// </summary>
        Vector3 Velocity { get; set; }

        /// <summary>
        /// Gets or sets the Yaw of the Entity.
        /// </summary>
        float Yaw { get; set; }

        /// <summary>
        /// Gets or sets the Pitch of the Entity.
        /// </summary>
        float Pitch { get; set; }

        /// <summary>
        /// Gets or sets if the Entity has been Despawned.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that once Despawned is set, it cannot be unset.
        /// </para>
        /// </remarks>
        bool Despawned { get; set; }

        /// <summary>
        /// Gets or sets the SpawnTime of the Entity.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that SpawnTime can only be set once.
        /// </para>
        /// </remarks>
        DateTime SpawnTime { get; set; }

        /// <summary>
        /// Gets the Metadata of the Entity.
        /// </summary>
        MetadataDictionary Metadata { get; }

        /// <summary>
        /// Gets the Size of the Entity.
        /// </summary>
        Size Size { get; }

        /// <summary>
        /// Gets the Axis-Aligned Bounding Box of the Entity.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This bounding box is a function of the Position and Size of the Entity.
        /// </para>
        /// </remarks>
        BoundingBox BoundingBox { get; }

        /// <summary>
        /// Convenience getter for the Entity Manager responsible for this Entity
        /// </summary>
        IEntityManager EntityManager { get; }

        /// <summary>
        /// Gets the Dimension in which this Entity is located.
        /// </summary>
        IDimension Dimension { get; }

        // TODO: This is a server-side only concern.
        /// <summary>
        /// 
        /// </summary>
        bool SendMetadataToClients { get; }

        // TODO: Remove EntityManager - it was already passed to the Entity constructor.
        // TODO: Add amount of time of the Update.
        /// <summary>
        /// Performs Entity-specific updating.
        /// </summary>
        /// <param name="entityManager"></param>
        void Update(IEntityManager entityManager);

        /// <summary>
        /// Acceleration due to gravity in meters per second squared.
        /// </summary>
        float AccelerationDueToGravity { get; }

        /// <summary>
        /// Velocity *= (1 - Drag) each second
        /// </summary>
        float Drag { get; }

        /// <summary>
        /// Terminal velocity in meters per second.
        /// </summary>
        float TerminalVelocity { get; }

        bool BeginUpdate();

        // TODO: Add Velocity parameter.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newPosition"></param>
        void EndUpdate(Vector3 newPosition);

        // TODO: collision point needs definition - colliding two AABBs results in a SURFACE of contact NOT a Point.
        /// <summary>
        /// Called when the Entity collides with Terrain.  Used to provide
        /// Entity-specific processing.
        /// </summary>
        /// <param name="collisionPoint"></param>
        /// <param name="collisionDirection"></param>
        void TerrainCollision(Vector3 collisionPoint, Vector3 collisionDirection);
    }
}
