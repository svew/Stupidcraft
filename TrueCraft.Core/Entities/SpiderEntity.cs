using System;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public class SpiderEntity : MobEntity
    {
        public SpiderEntity(IDimension dimension, IEntityManager entityManager) :
            base(dimension, entityManager)
        {
        }

        public override Size Size
        {
            get
            {
                return new Size(1.4, 0.9, 1.4);
            }
        }

        public override short MaxHealth
        {
            get
            {
                return 16;
            }
        }

        public override sbyte MobType
        {
            get
            {
                return 52;
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

