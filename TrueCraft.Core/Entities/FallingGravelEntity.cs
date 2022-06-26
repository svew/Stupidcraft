using System;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public class FallingGravelEntity : FallingSandEntity
    {
        public FallingGravelEntity(IDimension dimension, IEntityManager entityManager,
            Vector3 position) : base(dimension, entityManager, position)
        {
        }

        public override byte EntityType { get { return 71; } }
    }
}