using System;
using System.ComponentModel;
using Moq;
using NUnit.Framework;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Lighting;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Networking;
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

        private class TestEntity : IEntity
        {

            public event PropertyChangedEventHandler? PropertyChanged;

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

            public void Update(IEntityManager entityManager)
            {
                throw new NotImplementedException();
            }

            public IPacket SpawnPacket => throw new NotImplementedException();

            public int EntityID { get => 1; set => throw new NotImplementedException(); }
            public float Yaw { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public float Pitch { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public bool Despawned { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public DateTime SpawnTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public MetadataDictionary Metadata => throw new NotImplementedException();

            public IEntityManager EntityManager => throw new NotImplementedException();

            public IDimension Dimension => throw new NotImplementedException();

            public bool SendMetadataToClients => throw new NotImplementedException();
        }

        [Test]
        public void TestGravity()
        {
            IDimension dimension = BuildDimension();
            IPhysicsEngine physics = new PhysicsEngine(dimension);
            TestEntity entity = new TestEntity();
            entity.Position = new Vector3(0, 100, 0);
            entity.Velocity = Vector3.Zero;
            entity.AccelerationDueToGravity = 1;
            entity.Drag = 0;
            physics.AddEntity(entity);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(0, entity.Position.X);
            Assert.AreEqual(99, entity.Position.Y);
            Assert.AreEqual(0, entity.Position.Z);
            Assert.AreEqual(0, entity.Velocity.X);
            Assert.AreEqual(-1, entity.Velocity.Y);
            Assert.AreEqual(0, entity.Velocity.Z);

            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(0, entity.Position.X);
            Assert.AreEqual(97, entity.Position.Y);
            Assert.AreEqual(0, entity.Position.Z);
            Assert.AreEqual(0, entity.Velocity.X);
            Assert.AreEqual(-2, entity.Velocity.Y);
            Assert.AreEqual(0, entity.Velocity.Z);
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

            Assert.AreEqual(0, entity.Position.X);
            Assert.AreEqual(99, entity.Position.Y);
            Assert.AreEqual(0, entity.Position.Z);
            Assert.AreEqual(0, entity.Velocity.X);
            Assert.AreEqual(-1, entity.Velocity.Y);
            Assert.AreEqual(0, entity.Velocity.Z);
        }

        // Tests that the Terrain supports the Entity
        [Test]
        public void TestTerrainCollision()
        {
            IDimension dimension = BuildDimension();
            IPhysicsEngine physics = new PhysicsEngine(dimension);
            TestEntity entity = new TestEntity();
            entity.Size = new Size(0.6, 1.8, 0.6);
            double xPos = 10.9, zPos = 10.9;
            entity.Position = new Vector3(xPos, SurfaceHeight, zPos);
            entity.Velocity = Vector3.Zero;
            entity.AccelerationDueToGravity = 1;
            physics.AddEntity(entity);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(xPos, entity.Position.X);
            Assert.AreEqual(SurfaceHeight, entity.Position.Y);
            Assert.AreEqual(zPos, entity.Position.Z);
            Assert.AreEqual(Vector3.Zero, entity.Velocity);

            physics.Update(TimeSpan.FromSeconds(5));

            Assert.AreEqual(xPos, entity.Position.X);
            Assert.AreEqual(SurfaceHeight, entity.Position.Y);
            Assert.AreEqual(zPos, entity.Position.Z);
            Assert.AreEqual(Vector3.Zero, entity.Velocity);
        }

        [Test]
        public void TestExtremeTerrainCollision()
        {
            IDimension dimension = BuildDimension();
            IPhysicsEngine physics = new PhysicsEngine(dimension);
            TestEntity entity = new TestEntity();
            double origHeightAboveSurface = 5;
            entity.Position = new Vector3(0, SurfaceHeight + origHeightAboveSurface, 0);
            entity.Velocity = Vector3.Zero;
            entity.AccelerationDueToGravity = 10;
            physics.AddEntity(entity);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(0, entity.Position.X);
            Assert.AreEqual(SurfaceHeight, entity.Position.Y);
            Assert.AreEqual(0, entity.Position.Z);

            // The entity's velocity is the velocity required to go from its
            // previous position to its current position.  Not the original velocity,
            // but not yet zero either.
            Assert.AreEqual(new Vector3(0, -origHeightAboveSurface, 0), entity.Velocity);
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

            // Create a pillar
            for (int y = 0; y < 12; y++)
                dimension.SetBlockID(new GlobalVoxelCoordinates(1, y, 0), StoneBlockID);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(0, entity.Position.X);
            Assert.AreEqual(9, entity.Position.Y);
            Assert.AreEqual(0, entity.Position.Z);
            Assert.IsFalse(entity.CollisionOccured);
        }

        [Test]
        public void TestCollisionPoint()
        {
            IDimension dimension = BuildDimension();
            IPhysicsEngine physics = new PhysicsEngine(dimension);
            TestEntity entity = new TestEntity();
            entity.Position = new Vector3(0, 4.5, 0);
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
            entity.Velocity = new Vector3(1.5, 0, 0);
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
            entity.Velocity = new Vector3(1.5, 0, 1.5);
            physics.AddEntity(entity);
            dimension.SetBlockID(new GlobalVoxelCoordinates(0, 10, 0), StoneBlock.BlockID);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(-1, entity.Position.X);
            Assert.AreEqual(10, entity.Position.Y);
            Assert.AreEqual(-1, entity.Position.Z);
            Assert.AreEqual(0, entity.Velocity.X);
            Assert.AreEqual(0, entity.Velocity.Y);
            Assert.AreEqual(0, entity.Velocity.Z);
        }

        /// <summary>
        /// Tests a very odd condition involving a surprise launch into the air.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In my test world, I found a particular block that when I walk into the
        /// side of it, I get launched up into the air.  This test reproduces
        /// that with actual numbers and locations taken from the actual
        /// observed bug.
        /// </para>
        /// </remarks>
        [Test]
        public void CrazyJump()
        {
            //
            // Set up
            //
            IDimensionServer dimension = (IDimensionServer)BuildDimension();
            IPhysicsEngine physics = new PhysicsEngine(dimension);

            // Generate chunks at chunk coordinates (-1, 12) and (-1, 13)
            IChunk chunk12 = dimension.GetChunk(new GlobalChunkCoordinates(-1, 12), LoadEffort.Generate)!;
            IChunk chunk13 = dimension.GetChunk(new GlobalChunkCoordinates(-1, 13), LoadEffort.Generate)!;

            // Create a layer of dirt at y == 62.
            // This will place the entity's feet at y == 63.
            for (int x = 0; x < 16; x ++)
                for (int z = 0; z < 16; z ++)
                {
                    LocalVoxelCoordinates localCoords = new(x, 62, z);
                    chunk12.SetBlockID(localCoords, StoneBlockID);
                    chunk13.SetBlockID(localCoords, StoneBlockID);
                }

            // Place blocks at z == 192, x == -5 && x == -3
            dimension.SetBlockID(new GlobalVoxelCoordinates(-3, 63, 192), StoneBlockID);
            dimension.SetBlockID(new GlobalVoxelCoordinates(-5, 63, 192), StoneBlockID);

            Size entitySize = new Size(0.6, 1.8, 0.6);   // same size as Player Entity
            double entityY = 63 + entitySize.Height / 2;
            Vector3 entityStartPos = new Vector3(-4.34298322997483, entityY, 192.92777590726);
            Vector3 entityStartVel = new Vector3(0.0502763610985, 0,  -0.0520632516009488);

            TestEntity entity = new TestEntity();
            entity.Position = entityStartPos;
            entity.Velocity = entityStartVel;
            entity.Size = entitySize;
            entity.AccelerationDueToGravity = 1.6f;
            entity.Drag = 0.40f;
            entity.TerminalVelocity = 78.4f;
            physics.AddEntity(entity);

            //
            // Act
            //
            Console.Error.WriteLine($"Before: {entity.Position}");
            physics.Update(TimeSpan.FromSeconds(1));
            Console.Error.WriteLine($"After: {entity.Position}");
            Assert.AreEqual(0, entity.Velocity.Y);
            Assert.AreEqual(entityY, entity.Position.Y);
            Assert.True(entity.CollisionOccured);

            physics.Update(TimeSpan.FromSeconds(1));
            Assert.AreEqual(entityY, entity.Position.Y);
            Assert.AreEqual(0, entity.Velocity.Y);
        }
    }
}
