using System;
using TrueCraft.Core.AI;
using TrueCraft.Core.Physics;

namespace TrueCraft.Core.Entities
{
    public interface IMobEntity : IEntity, IAABBEntity
    {
        event EventHandler PathComplete;
        PathResult CurrentPath { get; set; }
        bool AdvancePath(TimeSpan time, bool faceRoute = true);
        IMobState CurrentState { get; set; }
        void Face(Vector3 target);
    }
}
