using System;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public class SlimeEntity : MobEntity
    {
        public byte SlimeSize { get; set; }

        public SlimeEntity(IDimension dimension, IEntityManager entityManager) :
            this(dimension, entityManager, 4)
        {
        }

        public SlimeEntity(IDimension dimension, IEntityManager entityManager, byte size) :
            base(dimension, entityManager)
        {
            SlimeSize = size;
        }

        public override MetadataDictionary Metadata
        {
            get
            {
                var meta = base.Metadata;
                meta[16] = new MetadataByte(SlimeSize);
                return meta;
            }
        }

        public override Size Size
        {
            get
            {
                return new Size(0.6 * SlimeSize);
            }
        }

        public override short MaxHealth
        {
            get
            {
                return (short)(Math.Pow(SlimeSize, 2));
            }
        }

        public override sbyte MobType
        {
            get
            {
                return 55;
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

