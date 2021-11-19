using System;
using TrueCraft.Core.Server;
using TrueCraft.Core.Entities;

namespace TrueCraft.Core.AI
{
    public interface IMobState
    {
        void Update(IMobEntity entity, IEntityManager manager);
    }
}
