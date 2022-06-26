using System;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public class WolfEntity : MobEntity
    {
        public WolfEntity(IDimension dimension, IEntityManager entityManager) :
            base(dimension, entityManager)
        {
        }

        public override Size Size
        {
            get
            {
                return new Size(0.6, 1.8, 0.6);
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
                return 95;
            }
        }
    }
}