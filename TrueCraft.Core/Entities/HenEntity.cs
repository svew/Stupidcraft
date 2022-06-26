using System;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public class HenEntity : MobEntity
    {
        public HenEntity(IDimension dimension, IEntityManager entityManager) :
            base(dimension, entityManager)
        {
        }

        public override Size Size
        {
            get
            {
                return new Size(0.4, 0.3, 0.4);
            }
        }

        public override short MaxHealth
        {
            get
            {
                return 4;
            }
        }

        public override sbyte MobType
        {
            get
            {
                return 93;
            }
        }
    }
}