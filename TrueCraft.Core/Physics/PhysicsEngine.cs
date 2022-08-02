using System;
using System.Collections.Generic;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Physics
{
    public class PhysicsEngine : IPhysicsEngine
    {
        private readonly IDimension _dimension;
        private readonly List<IEntity> _entities;
        private readonly object _entityLock;

        /// <summary>
        /// Any velocity vector components below this amount
        /// will be truncated to zero.
        /// </summary>
        private const double MinimumVelocity = 0.005;

        public PhysicsEngine(IDimension dimension)
        {
            _dimension = dimension;
            _entities = new List<IEntity>();
            _entityLock = new object();
        }

        public void AddEntity(IEntity entity)
        {
            lock (_entityLock)
                if (!_entities.Contains(entity))
                    _entities.Add(entity);
        }

        public void RemoveEntity(IEntity entity)
        {
            lock (_entityLock)
            {
                int index = _entities.IndexOf(entity);
                if (index >= 0)
                    _entities.RemoveAt(index);
            }
        }

        private Vector3 TruncateVelocity(double terminalVelocity, Vector3 velocity)
        {
            if (Math.Abs(velocity.X) < MinimumVelocity)
                velocity.X = 0;
            if (Math.Abs(velocity.Y) < MinimumVelocity)
                velocity.Y = 0;
            if (Math.Abs(velocity.Z) < MinimumVelocity)
                velocity.Z = 0;

            velocity.Clamp(terminalVelocity);
            return velocity;
        }

        /// <summary>
        /// Updates all the Entities managed by this Physics Engine.
        /// </summary>
        /// <param name="time">The amount of time since the last call to this Update.</param>
        public void Update(TimeSpan time)
        {
            double seconds = time.TotalSeconds;
            if (seconds == 0)
                return;

            lock(_entityLock)
            {
                foreach (IEntity entity in _entities)
                {
                    if (entity.BeginUpdate())
                    {
                        Vector3 velocity = entity.Velocity;
                        if (!IsGrounded(entity))
                            velocity -= new Vector3(0, entity.AccelerationDueToGravity * seconds, 0);
                        velocity *= 1 - entity.Drag * seconds;
                        velocity = TruncateVelocity(entity.TerminalVelocity, velocity);
                        Ray move = new Ray(entity.Position, velocity);
                        double nearestCollision = double.MaxValue;
                        GlobalVoxelCoordinates? collisionBlock = null;
                        BlockFace collisionFace = BlockFace.NegativeX;
                        BoundingBox? collisionTarget = null;

                        BoundingBox testBox = GetAABBVelocityBox(entity.BoundingBox, velocity);
                        int xmin = (int)(Math.Floor(testBox.Min.X));
                        int xmax = (int)(Math.Ceiling(testBox.Max.X));
                        int ymin = (int)(Math.Floor(testBox.Min.Y));
                        int ymax = (int)(Math.Ceiling(testBox.Max.Y));
                        int zmin = (int)(Math.Floor(testBox.Min.Z));
                        int zmax = (int)(Math.Ceiling(testBox.Max.Z));
                        for (int x = xmin; x <= xmax; x ++)
                            for (int z = zmin; z <= zmax; z ++)
                                for (int y = ymin; y <= ymax; y ++)
                                {
                                    GlobalVoxelCoordinates coords = new(x, y, z);
                                    BoundingBox? target = GetBoundingBox(_dimension, coords);
                                    if (!target.HasValue)
                                        continue;

                                    target = target.Value.OffsetBy((Vector3)coords);
                                    BoundingBox expandedTarget = target.Value.Expand(entity.Size);

                                    double collision = double.MaxValue;
                                    if (move.Intersects(expandedTarget, ref collision, ref collisionFace) && collision < nearestCollision)
                                    {
                                        nearestCollision = collision;
                                        collisionBlock = coords;
                                        collisionTarget = target.Value;
                                    }
                                }

                        if (collisionBlock is not null)
                        {
                            entity.TerrainCollision((Vector3)collisionBlock, move.Direction.Unit());
                            move = HandleCollision(entity.Size, move, seconds,
                                collisionBlock, collisionFace, collisionTarget!.Value);
                        }

                        entity.Velocity = move.Direction;
                        entity.EndUpdate(entity.Position + move.Direction * seconds);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether or not the Entity is vertically in contact with terrain.
        /// </summary>
        /// <param name="entity">The entity to check</param>
        /// <returns>True if the Entity is vertically in contact with terrain; false otherwise.</returns>
        private bool IsGrounded(IEntity entity)
        {
            BoundingBox bb = entity.BoundingBox;
            int xmin = (int)Math.Floor(bb.Min.X);
            int xmax = (int)Math.Floor(bb.Max.X);
            int zmin = (int)Math.Floor(bb.Min.Z);
            int zmax = (int)Math.Floor(bb.Max.Z);
            double y = bb.Min.Y;
            int ySupport = (int)(y == (int)Math.Floor(y) ? y - 1: Math.Floor(y));

            for (int x = xmin; x <= xmax; x ++)
                for (int z = zmin; z <= zmax; z ++)
                {
                    GlobalVoxelCoordinates coords = new(x, ySupport, z);
                    BoundingBox? support = GetBoundingBox(_dimension, coords);
                    if (!support.HasValue)
                        continue;

                    double supportTop = support.Value.Max.Y + ySupport;
                    double supportBottom = support.Value.Min.Y + ySupport;
                    // If the Entity's "feet" are inside the block or close enough
                    // to top to be considered in contact, then it is grounded.
                    if ((y <= supportTop && y > supportBottom) || Math.Abs(y - supportTop) < GameConstants.Epsilon)
                        return true;
                }

            return false;
        }

        /// <summary>
        /// Creates a new move Ray based upon the old one and the details of the collision.
        /// </summary>
        /// <param name="entitySize">The size of the Entity colliding with something</param>
        /// <param name="move">The original (unimpeded) version of the Entity's move.</param>
        /// <param name="seconds">The number of seconds of the update.</param>
        /// <param name="collisionBlock">The coordinates of the Block with which the Entity collided.</param>
        /// <param name="collisionFace">The face of the Block into which the Entity collided.</param>
        /// <param name="collisionTarget">The Bounding Box of the Block with which the Entity collided.</param>
        /// <returns>An updated Move as modified by the collision.</returns>
        private Ray HandleCollision(Size entitySize, Ray move, double seconds,
            GlobalVoxelCoordinates collisionBlock, BlockFace collisionFace,
            BoundingBox collisionTarget)
        {
            Vector3 velocity = move.Direction;

            switch(collisionFace)
            {
                case BlockFace.NegativeX:
                    velocity.X = collisionTarget.Min.X - move.Position.X - entitySize.Width / 2;
                    break;

                case BlockFace.PositiveX:
                    velocity.X = collisionTarget.Max.X - move.Position.X + entitySize.Width / 2;
                    break;

                case BlockFace.NegativeY:
                    velocity.Y = collisionTarget.Min.Y - move.Position.Y - entitySize.Height;
                    break;

                case BlockFace.PositiveY:
                    velocity.Y = collisionTarget.Max.Y - move.Position.Y;
                    break;

                case BlockFace.NegativeZ:
                    velocity.Z = collisionTarget.Min.Z - move.Position.Z - entitySize.Depth / 2;
                    break;

                case BlockFace.PositiveZ:
                    velocity.Z = collisionTarget.Max.Z - move.Position.Z + entitySize.Depth / 2;
                    break;
            }

            velocity /= seconds;
            return new Ray(move.Position, velocity);
        }

        private BoundingBox GetAABBVelocityBox(BoundingBox bb, Vector3 velocity)
        {
            Vector3 curMin = bb.Min;
            Vector3 curMax = bb.Max;

            Vector3 min = new Vector3(
                Math.Min(curMin.X, curMin.X + velocity.X),
                Math.Min(curMin.Y, curMin.Y + velocity.Y),
                Math.Min(curMin.Z, curMin.Z + velocity.Z)
            );
            Vector3 max = new Vector3(
                Math.Max(curMax.X, curMax.X + velocity.X),
                Math.Max(curMax.Y, curMax.Y + velocity.Y),
                Math.Max(curMax.Z, curMax.Z + velocity.Z)
            );
            return new BoundingBox(min, max);
        }

        private BoundingBox? GetBoundingBox(IDimension dimension, GlobalVoxelCoordinates coordinates)
        {
            byte id = dimension.GetBlockID(coordinates);
            if (id == AirBlock.BlockID) return null;

            IBlockProvider? provider = dimension.BlockRepository.GetBlockProvider(id);
            return provider?.BoundingBox;
        }
    }
}
