using System;
using TrueCraft.Core.Entities;
using System.Collections.Generic;
using TrueCraft.Core.Networking;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Server
{
    public interface IEntityManager
    {
        TimeSpan TimeSinceLastUpdate { get; }
        /// <summary>
        /// Adds an entity to the world and assigns it an entity ID.
        /// </summary>
        void SpawnEntity(IEntity entity);
        void DespawnEntity(IEntity entity);
        void FlushDespawns();
        IEntity GetEntityByID(int id);
        void Update();
        void SendEntitiesToClient(IRemoteClient client);
        IList<IEntity> EntitiesInRange(Vector3 center, float radius);
        IList<IRemoteClient> ClientsForEntity(IEntity entity);
    }
}
