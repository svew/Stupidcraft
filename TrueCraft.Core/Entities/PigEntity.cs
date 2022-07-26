using System;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public class PigEntity : MobEntity
    {
        public PigEntity(IDimension dimension, IEntityManager entityManager) :
            base(dimension, entityManager, 10, new Size(0.9))
        {
        }

        public override sbyte MobType
        {
            get
            {
                return 90;
            }
        }
    }
}