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
        protected ObjectEntity(IDimension dimension, IEntityManager entityManager,
            Size size, float accelerationDueToGravity, float drag, float terminalVelocity) :
            base(dimension, entityManager, size, accelerationDueToGravity, drag, terminalVelocity)
        {
        }

        // TODO: What is the meaning of this?
        public abstract byte EntityType { get; }

        // TODO: What is the meaning of this?
        public abstract int Data { get; }
    }
}
