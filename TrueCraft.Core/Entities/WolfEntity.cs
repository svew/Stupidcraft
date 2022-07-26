using System;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public class WolfEntity : MobEntity
    {
        public WolfEntity(IDimension dimension, IEntityManager entityManager) :
            base(dimension, entityManager, 10,
                new Size(0.6, 1.8, 0.6))      // TODO: Fix Size: A Wolf is not the same size as a Player
        {
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