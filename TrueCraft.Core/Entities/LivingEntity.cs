using System;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    // TODO: factor out an ILivingEntity interface.
    public abstract class LivingEntity : Entity
    {
        private readonly short _maxHealth;
        protected short _air;
        protected short _health;
        protected float _headYaw;

        protected LivingEntity(IDimension dimension, IEntityManager entityManager,
            short maxHealth, Size size, float accelerationDueToGravity,
            float drag, float terminalVelocity) :
            base(dimension, entityManager, size, accelerationDueToGravity, drag,
                terminalVelocity)
        {
            _maxHealth = maxHealth;
            Health = MaxHealth;
        }

        public short Air
        {
            get { return _air; }
            set
            {
                if (_air == value)
                    return;
                _air = value;
                OnPropertyChanged();
            }
        }

        public short Health
        {
            get { return _health; }
            set
            {
                if (_health == value)
                    return;
                _health = value;
                OnPropertyChanged();
            }
        }

        public float HeadYaw
        {
            get { return _headYaw; }
            set
            {
                if (_headYaw == value)
                    return;
                _headYaw = value;
                OnPropertyChanged();
            }
        }

        public override bool SendMetadataToClients
        {
            get
            {
                return true;
            }
        }

        public virtual short MaxHealth { get => _maxHealth; }
    }
}
