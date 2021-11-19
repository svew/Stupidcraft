using System;
using TrueCraft.Core.Entities;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Physics
{
    public interface IPhysicsEngine
    {
        IWorld World { get; set; }
        void AddEntity(IPhysicsEntity entity);
        void RemoveEntity(IPhysicsEntity entity);
        void Update(TimeSpan time);
    }
}
