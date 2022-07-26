using System;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Server;
using TrueCraft.Core.AI;
using TrueCraft.Core.Physics;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public abstract class MobEntity : LivingEntity, IMobEntity
    {
        private PathResult? _currentPath = null;
        private double _speed;
        private IMobState? _mobState = null;

        protected MobEntity(IDimension dimension, IEntityManager entityManager,
            short maxHealth, Size size) :
            base(dimension, entityManager, maxHealth,
                size,
                1.6f,               // Acceleration Due To Gravity
                0.40f,              // Drag
                78.4f)              // Terminal Velocity
        {
            _speed = 4;
            CurrentState = new WanderState();
        }

        public event EventHandler? PathComplete;

        public override IPacket SpawnPacket
        {
            get
            {
                return new SpawnMobPacket(EntityID, MobType,
                    MathHelper.CreateAbsoluteInt(Position.X),
                    MathHelper.CreateAbsoluteInt(Position.Y),
                    MathHelper.CreateAbsoluteInt(Position.Z),
                    MathHelper.CreateRotationByte(Yaw),
                    MathHelper.CreateRotationByte(Pitch),
                    Metadata);
            }
        }

        // TODO: Make an enum for this.
        // TODO: Why isn't this in IMobEntity?
        public abstract sbyte MobType { get; }

        // TODO: Does Beta 1.7.3 have Friendly/Neutral/Hostile?
        // TODO: Why isn't this in IMobEntity?
        public virtual bool Friendly { get { return true; } }

        public PathResult? CurrentPath
        {
            get => _currentPath;
            set
            {
                if (_currentPath == value)
                    return;
                _currentPath = value;
                OnPropertyChanged();
            }
        }

        // TODO: Why isn't this in IMobEntity?
        /// <summary>
        /// Mob's current speed in m/s.
        /// </summary>
        public virtual double Speed
        {
            get => _speed;
            set
            {
                if (_speed == value)
                    return;
                _speed = value;
                OnPropertyChanged();
            }
        }

        public IMobState? CurrentState
        {
            get => _mobState;
            set
            {
                if (_mobState == value)
                    return;
                _mobState = value;
                OnPropertyChanged();
            }
        }

        public void Face(Vector3 target)
        {
            var diff = target - Position;
            Yaw = (float)MathHelper.RadiansToDegrees(-(Math.Atan2(diff.X, diff.Z) - Math.PI) + Math.PI); // "Flip" over the 180 mark
        }

        public bool AdvancePath(TimeSpan time, bool faceRoute = true)
        {
            var modifier = time.TotalSeconds * Speed;
            if (CurrentPath != null)
            {
                // Advance along path
                var target = (Vector3)CurrentPath[CurrentPath.Index];
                target += new Vector3(Size.Width / 2, 0, Size.Depth / 2); // Center it
                target.Y = Position.Y; // TODO: Find better way of doing this
                if (faceRoute)
                    Face(target);
                var lookAt = Vector3.Forwards.Transform(Matrix.CreateRotationY(MathHelper.ToRadians(-(Yaw - 180) + 180)));
                lookAt *= modifier;
                Velocity = new Vector3(lookAt.X, Velocity.Y, lookAt.Z);
                if (Position.DistanceTo(target) < Velocity.Distance)
                {
                    Position = target;
                    Velocity = Vector3.Zero;
                    CurrentPath.Index++;
                    if (CurrentPath.Index >= CurrentPath.Count)
                    {
                        CurrentPath = null;
                        PathComplete?.Invoke(this, EventArgs.Empty);
                        return true;
                    }
                }
            }
            return false;
        }

        public override void Update(IEntityManager entityManager)
        {
            if (_mobState is not null)
                _mobState.Update(this, entityManager);
            else
                AdvancePath(entityManager.TimeSinceLastUpdate);
            base.Update(entityManager);
        }
    }
}

