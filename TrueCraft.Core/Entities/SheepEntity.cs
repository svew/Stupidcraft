using System;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public class SheepEntity : MobEntity
    {
        public SheepEntity(IDimension dimension, IEntityManager entityManager) :
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
                return 8;
            }
        }

        public override sbyte MobType
        {
            get
            {
                return 91;
            }
        }
    }
}