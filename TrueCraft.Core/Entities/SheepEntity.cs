using System;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public class SheepEntity : MobEntity
    {
        public SheepEntity(IDimension dimension, IEntityManager entityManager) :
            base(dimension, entityManager, 8, new Size(0.9, 1.3, 0.9))  // TODO: sheep is taller than long?
        {
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