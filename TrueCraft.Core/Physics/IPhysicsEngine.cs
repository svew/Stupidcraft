using System;
using TrueCraft.Core.Entities;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Physics
{
    public interface IPhysicsEngine
    {
        /// <summary>
        /// Adds an Entity to the set of Entities being managed by this Physics Engine.
        /// </summary>
        /// <param name="entity">The Entity to add.</param>
        void AddEntity(IEntity entity);

        /// <summary>
        /// Removes an Entity from the set of Entities managed by this Physics Engine.
        /// </summary>
        /// <param name="entity"></param>
        void RemoveEntity(IEntity entity);

        /// <summary>
        /// Called to periodically update the set of Entities being managaed by this Physics Engine.
        /// </summary>
        /// <param name="time">The time passed since the last call to Update.  Ideally,
        /// this is equal to the tick time.</param>
        void Update(TimeSpan time);

        /// <summary>
        /// Checks if the given Entity is on the Ground.
        /// </summary>
        /// <param name="entity">The Entity to check.</param>
        /// <returns>True if the entity is in contact with the ground; false otherwise.</returns>
        bool IsGrounded(IEntity entity);
    }
}
