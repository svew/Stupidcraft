using System;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public class CowEntity : MobEntity
    {
        public CowEntity(IDimension dimension, IEntityManager entityManager) :
            base(dimension, entityManager)
        {
        }

        public override Size Size
        {
            get
            {
                return new Size(0.9, 1.3, 0.9);
            }
        }

        public override short MaxHealth
        {
            get
            {
                return 10;
            }
        }

        public override sbyte MobType
        {
            get
            {
                return 92;
            }
        }
    }
}