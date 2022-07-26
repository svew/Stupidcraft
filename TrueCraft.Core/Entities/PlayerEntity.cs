using System;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    // TODO: factor out interface IPlayerEntity
    public class PlayerEntity : LivingEntity
    {
        public const double Width = 0.6;
        public const double Height = 1.62;
        public const double Depth = 0.6;

        private readonly string _username;

        private Vector3 _oldPosition;
        private short _selectedSlot;
        private Vector3 _spawnPoint;

        private bool _isSprinting;
        private bool _isCrouching;

        public PlayerEntity(IDimension dimension, IEntityManager entityManager,
            string username) :
            base(dimension, entityManager, 20,   // Max Health
                new Size(Width, Height, Depth),
                1.6f,                           // Acceleration Due To Gravity
                0.4f,                           // Drag
                78.4f)                          // Terminal Velocity
        {
            _username = username;
            _isSprinting = false;
            _isCrouching = false;
        }


        public override IPacket SpawnPacket
        {
            get
            {
                return new SpawnPlayerPacket(EntityID, _username,
                    MathHelper.CreateAbsoluteInt(Position.X),
                    MathHelper.CreateAbsoluteInt(Position.Y),
                    MathHelper.CreateAbsoluteInt(Position.Z),
                    MathHelper.CreateRotationByte(Yaw),
                    MathHelper.CreateRotationByte(Pitch), 0 /* Note: current item is set through other means */);
            }
        }

        public override Size Size
        {
            // TODO: This will change when Crouching.
            get { return new Size(Width, Height, Depth); }
        }

        public bool IsSprinting
        {
            get => _isSprinting;
            set
            {
                if (_isSprinting == value)
                    return;
                _isSprinting = value;
                OnPropertyChanged();
            }
        }

        public bool IsCrouching
        {
            get => _isCrouching;
            set
            {
                if (_isCrouching == value)
                    return;
                _isCrouching = value;
                OnPropertyChanged();
            }
        }

        public Vector3 OldPosition
        {
            get
            {
                return _oldPosition;
            }
            private set
            {
                if (_oldPosition == value)
                    return;
                _oldPosition = value;
                OnPropertyChanged();
            }
        }

        public override Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                if (_position == value)
                    return;
                base.Position = value;
                OldPosition = _position;
            }
        }

        public short SelectedSlot
        {
            get { return _selectedSlot; }
            set
            {
                if (_selectedSlot == value)
                    return;
                _selectedSlot = value;
                OnPropertyChanged();
            }
        }

        public Vector3 SpawnPoint
        {
            get { return _spawnPoint; }
            set
            {
                if (_spawnPoint == value)
                    return;
                _spawnPoint = value;
                OnPropertyChanged();
            }
        }

        // TODO: Can zombies pick up Items?  Perhaps, this needs to be in the base class.
        public event EventHandler<EntityEventArgs>? PickUpItem;
        public void OnPickUpItem(ItemEntity item)
        {
            PickUpItem?.Invoke(this, new EntityEventArgs(item));
        }
    }
}
