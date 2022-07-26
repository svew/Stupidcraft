using System;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public class CreeperEntity : MobEntity
    {
        public CreeperEntity(IDimension dimension, IEntityManager entityManager) :
            base(dimension, entityManager, 20, new Size(0.6, 1.8, 0.6))
        {
        }

        public override sbyte MobType
        {
            get
            {
                return 50;
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