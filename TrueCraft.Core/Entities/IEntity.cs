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
        IEntityManager EntityManager { get; set; }
        IDimension Dimension { get; set;  }
        bool SendMetadataToClients { get; }
        void Update(IEntityManager entityManager);
    }
}
