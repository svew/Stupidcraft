using System;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Entities
{
    public class PlayerEntity : LivingEntity
    {
        private readonly string _username;

        public PlayerEntity(IDimension dimension, IEntityManager entityManager, string username) :
            base(dimension, entityManager)
        {
            _username = username;
        }

        public const double Width = 0.6;
        public const double Height = 1.62;
        public const double Depth = 0.6;

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
            get { return new Size(Width, Height, Depth); }
        }

        public override short MaxHealth
        {
            get { return 20; }
        }

        public bool IsSprinting { get; set; }
        public bool IsCrouching { get; set; }
        public double PositiveDeltaY { get; set; }

        private Vector3 _OldPosition;
        public Vector3 OldPosition
        {
            get
            {
                return _OldPosition;
            }
            private set
            {
                _OldPosition = value;
            }
        }

        public override Vector3 Position
        {
            get
            {
                return _Position;
            }
            set
            {
                _OldPosition = _Position;
                _Position = value;
                OnPropertyChanged("Position");
            }
        }

        protected short _SelectedSlot;
        public short SelectedSlot
        {
            get { return _SelectedSlot; }
            set
            {
                _SelectedSlot = value;
                OnPropertyChanged("SelectedSlot");
            }
        }

        public ItemStack ItemInMouse { get; set; }

        protected Vector3 _SpawnPoint;
        public Vector3 SpawnPoint
        {
            get { return _SpawnPoint; }
            set
            {
                _SpawnPoint = value;
                OnPropertyChanged("SpawnPoint");
            }
        }

        public event EventHandler<EntityEventArgs>? PickUpItem;
        public void OnPickUpItem(ItemEntity item)
        {
            PickUpItem?.Invoke(this, new EntityEventArgs(item));
        }
    }
}
