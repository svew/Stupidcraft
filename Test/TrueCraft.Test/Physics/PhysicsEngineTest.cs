using System;
using Moq;
using NUnit.Framework;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Physics;
using TrueCraft.Core.World;
using TrueCraft.TerrainGen;

namespace TrueCraft.Core.Test.Physics
{
    // TODO: Note dependency upon the FlatlandGenerator
    [TestFixture]
    public class PhysicsEngineTest
    {
        private const int _testSeed = 314159;

        private IBlockRepository _blockRepository;

        private IBlockPhysicsProvider _blockPhysicsProvider;

        public PhysicsEngineTest()
        {
            Mock<IBlockProvider> mockProvider = new Mock<IBlockProvider>(MockBehavior.Strict);
            mockProvider.Setup(x => x.ID).Returns(3);

            Mock<IBlockRepository> mockRepository = new Mock<IBlockRepository>(MockBehavior.Strict);
            mockRepository.Setup(x => x.GetBlockProvider(It.Is<byte>(b => b == 3))).Returns(mockProvider.Object);
            _blockRepository = mockRepository.Object;

            // Note: dependency upon BoundingBox class
            Mock<IBlockPhysicsProvider> mockPhysicsProvider = new Mock<IBlockPhysicsProvider>(MockBehavior.Strict);
            mockPhysicsProvider.Setup(x => x.GetBoundingBox(It.IsAny<IDimension>(), It.IsAny<GlobalVoxelCoordinates>())).Returns(new BoundingBox(Vector3.Zero, Vector3.One));
            _blockPhysicsProvider = mockPhysicsProvider.Object;
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
            IDimension dimension = new TrueCraft.World.Dimension(string.Empty, DimensionID.Overworld, new FlatlandGenerator(_testSeed), _blockRepository);
            IPhysicsEngine physics = new PhysicsEngine(dimension, _blockPhysicsProvider);
            var entity = new TestEntity();
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
            IDimension dimension = new TrueCraft.World.Dimension(string.Empty, DimensionID.Overworld, new FlatlandGenerator(_testSeed), _blockRepository);
            IPhysicsEngine physics = new PhysicsEngine(dimension, _blockPhysicsProvider);
            var entity = new TestEntity();
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
            IDimension dimension = new TrueCraft.World.Dimension(string.Empty, DimensionID.Overworld, new FlatlandGenerator(_testSeed), _blockRepository);
            IPhysicsEngine physics = new PhysicsEngine(dimension, _blockPhysicsProvider);
            var entity = new TestEntity();
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
            IDimension dimension = new TrueCraft.World.Dimension(string.Empty, DimensionID.Overworld, new FlatlandGenerator(_testSeed), _blockRepository);
            IPhysicsEngine physics = new PhysicsEngine(dimension, _blockPhysicsProvider);
            var entity = new TestEntity();
            entity.Position = new Vector3(0, 4, 0);
            entity.AccelerationDueToGravity = 10;
            physics.AddEntity(entity);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(4, entity.Position.Y);
        }

        [Test]
        public void TestAdjacentFall()
        {
            // Tests an entity that falls alongside a wall

            IDimension dimension = new TrueCraft.World.Dimension(string.Empty, DimensionID.Overworld, new FlatlandGenerator(_testSeed), _blockRepository);
            IPhysicsEngine physics = new PhysicsEngine(dimension, _blockPhysicsProvider);
            var entity = new TestEntity();
            entity.Position = new Vector3(0, 10, 0);
            entity.AccelerationDueToGravity = 1;
            physics.AddEntity(entity);

            // Create a wall
            for (int y = 0; y < 12; y++)
                dimension.SetBlockID(new GlobalVoxelCoordinates(1, y, 0), StoneBlock.BlockID);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(9, entity.Position.Y);
            Assert.IsFalse(entity.CollisionOccured);
        }

        [Test]
        public void TestCollisionPoint()
        {
            IDimension dimension = new TrueCraft.World.Dimension(string.Empty, DimensionID.Overworld, new FlatlandGenerator(_testSeed), _blockRepository);
            IPhysicsEngine physics = new PhysicsEngine(dimension, _blockPhysicsProvider);
            var entity = new TestEntity();
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
            IDimension dimension = new TrueCraft.World.Dimension(string.Empty, DimensionID.Overworld, new FlatlandGenerator(_testSeed), _blockRepository);
            IPhysicsEngine physics = new PhysicsEngine(dimension, _blockPhysicsProvider);
            var entity = new TestEntity();
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
            IDimension dimension = new TrueCraft.World.Dimension(string.Empty, DimensionID.Overworld, new FlatlandGenerator(_testSeed), _blockRepository);
            IPhysicsEngine physics = new PhysicsEngine(dimension, _blockPhysicsProvider);
            var entity = new TestEntity();
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