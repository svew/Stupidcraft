using System;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public class ZombieEntity : MobEntity
    {
        public ZombieEntity(IDimension dimension, IEntityManager entityManager) :
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
                return 20;
            }
        }

        public override sbyte MobType
        {
            get
            {
                return 54;
            }
        }

        public override bool Friendly
        {
            get
            {
                return false;
            }
        }
    }
}