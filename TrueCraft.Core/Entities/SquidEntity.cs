using System;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public class SquidEntity : MobEntity
    {
        public SquidEntity(IDimension dimension, IEntityManager entityManager) :
            base(dimension, entityManager, 10, new Size(0.95))  // TODO: Size seems wrong...
        {
        }

        public override sbyte MobType
        {
            get
            {
                return 94;
            }
        }
    }
}