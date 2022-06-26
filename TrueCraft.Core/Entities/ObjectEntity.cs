using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public abstract class ObjectEntity : Entity
    {
        protected ObjectEntity(IDimension dimension, IEntityManager entityManager) : base(dimension, entityManager)
        {
        }

        public abstract byte EntityType { get; }
        public abstract int Data { get; }
    }
}
