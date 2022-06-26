using System;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public class GhastEntity : MobEntity
    {
        public GhastEntity(IDimension dimension, IEntityManager entityManager) :
            base(dimension, entityManager)
        {
        }

        public override Size Size
        {
            get
            {
                return new Size(4.0);
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
                return 56;
            }
        }

        public override bool BeginUpdate()
        {
            // Ghasts can fly, no need to work out gravity
            // TODO: Think about how to deal with walls and such
            return false;
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