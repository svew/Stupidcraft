using System;
using System.ComponentModel;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public interface IEntity : INotifyPropertyChanged
    {
        IPacket SpawnPacket { get; }
        int EntityID { get; set; }
        Vector3 Position { get; set; }
        float Yaw { get; set; }
        float Pitch { get; set; }
        bool Despawned { get; set; }
        DateTime SpawnTime { get; set; }
        MetadataDictionary Metadata { get; }
        Size Size { get; }

        /// <summary>
        /// Convenience getter for the Entity Manager responsible for this Entity
        /// </summary>
        IEntityManager EntityManager { get; }

        /// <summary>
        /// Gets the Dimension in which this Entity is located.
        /// </summary>
        IDimension Dimension { get; }

        bool SendMetadataToClients { get; }
        void Update(IEntityManager entityManager);
    }
}
