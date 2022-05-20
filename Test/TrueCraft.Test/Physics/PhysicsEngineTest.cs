﻿using System;
using Moq;
using NUnit.Framework;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Lighting;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Physics;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;
using TrueCraft.TerrainGen;
using TrueCraft.Test.World;
using TrueCraft.World;

namespace TrueCraft.Core.Test.Physics
{
    [TestFixture]
    public class PhysicsEngineTest
    {
        private const byte StoneBlockID = 1;

        private const int SurfaceHeight = 1;

        private readonly IBlockRepository _blockRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IEntityManager _entityManager;

        public PhysicsEngineTest()
        {
            Mock<IBlockProvider> mockStoneBlock = new Mock<IBlockProvider>(MockBehavior.Strict);
            mockStoneBlock.Setup(x => x.ID).Returns(StoneBlockID);
            mockStoneBlock.Setup(x => x.BoundingBox).Returns(new BoundingBox(Vector3.Zero, Vector3.One));

            Mock<IBlockRepository> mockBlockRepository = new Mock<IBlockRepository>(MockBehavior.Strict);
            mockBlockRepository.Setup(x => x.GetBlockProvider(It.Is<byte>(b => b == StoneBlockID))).Returns(mockStoneBlock.Object);
            _blockRepository = mockBlockRepository.Object;

            Mock<IItemRepository> mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);
            _itemRepository = mockItemRepository.Object;

            Mock<IEntityManager> mockEntityManager = new Mock<IEntityManager>(MockBehavior.Strict);
            _entityManager = mockEntityManager.Object;
        }

        private IDimension BuildDimension()
        {
            IDimensionServer rv = new FakeDimension(_blockRepository,
                _itemRepository, _entityManager);

            // Generate a chunk
            IChunk? chunk = rv.GetChunk(new GlobalChunkCoordinates(0, 0), LoadEffort.Generate);
            for (int x = 0; x < Chunk.Width; x++)
                for (int z = 0; z < Chunk.Depth; z++)
                    chunk?.SetBlockID(new LocalVoxelCoordinates(x, 0, z), StoneBlockID);

            return rv;
        }

        private class TestEntity : IAABBEntity
        {
            public TestEntity()
            {
                TerminalVelocity = 10;
                Size = new Size(1);
                CollisionOccured = false;
            }

            public bool BeginUpdate()
            {
                return true;
            }

            public void EndUpdate(Vector3 newPosition)
            {
                Position = newPosition;
            }

            public Vector3 Position { get; set; }
            public Vector3 Velocity { get; set; }
            public float AccelerationDueToGravity { get; set; }
            public float Drag { get; set; }
            public float TerminalVelocity { get; set; }
            public Vector3 CollisionPoint { get; set; }
            public bool CollisionOccured { get; set; }

            public void TerrainCollision(Vector3 collisionPoint, Vector3 collisionDirection)
            {
                CollisionPoint = collisionPoint;
                CollisionOccured = true;
            }

            public BoundingBox BoundingBox
            {
                get
                {
                    return new BoundingBox(Position, Position + Size);
                }
            }

            public Size Size { get; set; }
        }

        [Test]
        public void TestGravity()
        {
            IDimension dimension = BuildDimension();
            IPhysicsEngine physics = new PhysicsEngine(dimension);
            TestEntity entity = new TestEntity();
            entity.Position = new Vector3(0, 100, 0);
            entity.AccelerationDueToGravity = 1;
            entity.Drag = 0;
            physics.AddEntity(entity);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(99, entity.Position.Y);

            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(97, entity.Position.Y);
        }

        [Test]
        public void TestDrag()
        {
            IDimension dimension = BuildDimension();
            IPhysicsEngine physics = new PhysicsEngine(dimension);
            TestEntity entity = new TestEntity();
            entity.Position = new Vector3(0, 100, 0);
            entity.AccelerationDueToGravity = 0;
            entity.Drag = 0.5f;
            entity.Velocity = Vector3.Down * 2;
            physics.AddEntity(entity);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(99, entity.Position.Y);
        }

        [Test]
        public void TestTerrainCollision()
        {
            IDimension dimension = BuildDimension();
            IPhysicsEngine physics = new PhysicsEngine(dimension);
            TestEntity entity = new TestEntity();
            entity.Size = new Size(0.6, 1.8, 0.6);
            entity.Position = new Vector3(-10.9, 4, -10.9);
            entity.AccelerationDueToGravity = 1;
            physics.AddEntity(entity);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(4, entity.Position.Y);

            physics.Update(TimeSpan.FromSeconds(5));

            Assert.AreEqual(4, entity.Position.Y);
        }

        [Test]
        public void TestExtremeTerrainCollision()
        {
            IDimension dimension = BuildDimension();
            IPhysicsEngine physics = new PhysicsEngine(dimension);
            TestEntity entity = new TestEntity();
            entity.Position = new Vector3(0, SurfaceHeight + 5, 0);
            entity.AccelerationDueToGravity = 10;
            physics.AddEntity(entity);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(SurfaceHeight, entity.Position.Y);
        }

        [Test]
        public void TestAdjacentFall()
        {
            // Tests an entity that falls alongside a wall
            IDimension dimension = BuildDimension();
            IPhysicsEngine physics = new PhysicsEngine(dimension);
            TestEntity entity = new TestEntity();
            entity.Position = new Vector3(0, 10, 0);
            entity.AccelerationDueToGravity = 1;
            physics.AddEntity(entity);

            // Create a wall
            for (int y = 0; y < 12; y++)
                dimension.SetBlockID(new GlobalVoxelCoordinates(1, y, 0), StoneBlockID);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(9, entity.Position.Y);
            Assert.IsFalse(entity.CollisionOccured);
        }

        [Test]
        public void TestCollisionPoint()
        {
            IDimension dimension = BuildDimension();
            IPhysicsEngine physics = new PhysicsEngine(dimension);
            TestEntity entity = new TestEntity();
            entity.Position = new Vector3(0, 5, 0);
            entity.AccelerationDueToGravity = 1;
            entity.Drag = 0;
            physics.AddEntity(entity);

            dimension.SetBlockID(new GlobalVoxelCoordinates(0, 4, 0), StoneBlock.BlockID);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(new Vector3(0, 4, 0), entity.CollisionPoint);
        }

        [Test]
        public void TestHorizontalCollision()
        {
            IDimension dimension = BuildDimension();
            IPhysicsEngine physics = new PhysicsEngine(dimension);
            TestEntity entity = new TestEntity();
            entity.Position = new Vector3(0, 5, 0);
            entity.AccelerationDueToGravity = 0;
            entity.Drag = 0;
            entity.Velocity = new Vector3(1, 0, 0);
            physics.AddEntity(entity);
            dimension.SetBlockID(new GlobalVoxelCoordinates(1, 5, 0), StoneBlock.BlockID);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(0, entity.Position.X);
            Assert.AreEqual(0, entity.Velocity.X);
        }

        [Test]
        public void TestCornerCollision()
        {
            IDimension dimension = BuildDimension();
            IPhysicsEngine physics = new PhysicsEngine(dimension);
            TestEntity entity = new TestEntity();
            entity.Position = new Vector3(-1, 10, -1);
            entity.AccelerationDueToGravity = 0;
            entity.Drag = 0;
            entity.Velocity = new Vector3(1, 0, 1);
            physics.AddEntity(entity);
            dimension.SetBlockID(new GlobalVoxelCoordinates(0, 10, 0), StoneBlock.BlockID);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(-1, entity.Position.X);
            Assert.AreEqual(-1, entity.Position.Z);
            Assert.AreEqual(0, entity.Velocity.X);
            Assert.AreEqual(0, entity.Velocity.Z);
        }
    }
}