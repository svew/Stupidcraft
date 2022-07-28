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
        /// <summary>
        /// A constant to check if two doubles are "close enough" to equal
        /// </summary>
        /// <remarks>This is much larger than double.Epsilon.</remarks>
        private const double Epsilon = 1e-8;

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
                    double hw = Size.Width * 0.5;
                    double hh = Size.Height * 0.5;
                    double hd = Size.Depth * 0.5;
                    Vector3 min = new Vector3(Position.X - hw, Position.Y - hh, Position.Z - hd);
                    Vector3 max = new Vector3(Position.X + hw, Position.Y + hh, Position.Z + hd);
                    return new BoundingBox(min, max);
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
            double yPos = SurfaceHeight + entity.Size.Height / 2;
            entity.Position = new Vector3(xPos, yPos, zPos);
            entity.Velocity = Vector3.Zero;
            entity.AccelerationDueToGravity = 1;
            physics.AddEntity(entity);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(xPos, entity.Position.X);
            Assert.AreEqual(yPos, entity.Position.Y);
            Assert.AreEqual(zPos, entity.Position.Z);
            Assert.True(Math.Abs(entity.Velocity.X) < Epsilon);
            Assert.True(Math.Abs(entity.Velocity.Y) < Epsilon);
            Assert.True(Math.Abs(entity.Velocity.Z) < Epsilon);

            physics.Update(TimeSpan.FromSeconds(5));

            Assert.AreEqual(xPos, entity.Position.X);
            Assert.AreEqual(yPos, entity.Position.Y);
            Assert.AreEqual(zPos, entity.Position.Z);
            Assert.True(Math.Abs(entity.Velocity.X) < Epsilon);
            Assert.True(Math.Abs(entity.Velocity.Y) < Epsilon);
            Assert.True(Math.Abs(entity.Velocity.Z) < Epsilon);
        }

        [Test]
        public void TestExtremeTerrainCollision()
        {
            IDimension dimension = BuildDimension();
            IPhysicsEngine physics = new PhysicsEngine(dimension);
            TestEntity entity = new TestEntity();
            double halfHeight = entity.Size.Height / 2;
            double origHeightAboveSurface = 5;
            entity.Position = new Vector3(0, SurfaceHeight + origHeightAboveSurface + halfHeight, 0);
            entity.Velocity = Vector3.Zero;
            entity.AccelerationDueToGravity = 10;
            physics.AddEntity(entity);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            Assert.AreEqual(0, entity.Position.X);
            Assert.AreEqual(SurfaceHeight + halfHeight, entity.Position.Y);
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
            entity.Position = new Vector3(0, 4.5 + entity.Size.Height / 2, 0);
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

            //
            // Assertions
            //

            // The centre of the entity should be half the entity's width from
            // the near edge of the block at x==1.
            Assert.AreEqual(1 - entity.Size.Width / 2, entity.Position.X);

            // The Entity's y and z positions should remain unchanged
            Assert.AreEqual(5, entity.Position.Y);
            Assert.AreEqual(0, entity.Position.Z);

            // The Entity's x velocity should be that which was required to move
            // from it's centre at x == 0 to being in contact with the block at x == 1.
            Assert.AreEqual(1 - entity.Size.Width / 2, entity.Velocity.X);

            // The Entity's y and z velocity components should remain unchanged.
            Assert.AreEqual(0, entity.Velocity.Y);
            Assert.AreEqual(0, entity.Velocity.Z);
        }

        // Start an Entity away from a block.
        // Move it diagonally towards the block such that the corner of the
        // Entity's AABB contacts the corner of the Block's AABB.
        [Test]
        public void TestCornerCollision()
        {
            IDimension dimension = BuildDimension();
            IPhysicsEngine physics = new PhysicsEngine(dimension);
            TestEntity entity = new TestEntity();
            double xPos = -1, yPos = 10, zPos = -1;
            entity.Position = new Vector3(xPos, yPos, zPos);
            entity.AccelerationDueToGravity = 0;
            entity.Drag = 0;
            double xVel = 1.5, yVel = 0, zVel = 1.5;
            entity.Velocity = new Vector3(xVel, yVel, zVel);
            physics.AddEntity(entity);
            int xBlock = 0, yBlock = (int)yPos, zBlock = 0;
            dimension.SetBlockID(new GlobalVoxelCoordinates(xBlock, yBlock, zBlock), StoneBlock.BlockID);

            // Test
            physics.Update(TimeSpan.FromSeconds(1));

            //
            // Asssertions
            //
            // In the x-direction, the entity should be stopped half its width
            // before the block.
            Assert.AreEqual(xBlock - entity.Size.Width / 2, entity.Position.X);
            // The y location should be unchanged.
            Assert.AreEqual(yPos, entity.Position.Y);
            // In the z-direction, the entity should be stopped half its depth
            // before the block.
            Assert.AreEqual(zBlock - entity.Size.Depth / 2, entity.Position.Z);

            // The x-velocity should be that which was required to move from
            // the initial position to the final position in one unit of time.
            Assert.AreEqual(xBlock - xPos - entity.Size.Width / 2, entity.Velocity.X);
            // The y-velocity should remain unchanged.
            Assert.AreEqual(yVel, entity.Velocity.Y);
            // The z-velocity should be that which was required to move from
            // the initial position to the final position in one unit of time.
            Assert.AreEqual(zBlock - zPos - entity.Size.Depth / 2, entity.Velocity.Z);
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
            double xPos = -4.34298322997483;
            double yPos = 63 + entitySize.Height / 2;
            double zPos = 192.92777590726;
            Vector3 entityStartPos = new Vector3(xPos, yPos, zPos);
            double xVel = 0.0502763610985;
            double yVel = 0;
            double zVel = -0.0520632516009488;
            Vector3 entityStartVel = new Vector3(xVel, yVel,  zVel);

            TestEntity entity = new TestEntity();
            entity.Position = entityStartPos;
            entity.Velocity = entityStartVel;
            entity.Size = entitySize;
            entity.AccelerationDueToGravity = 1.6f;
            entity.Drag = 0.0f;  // In the scenario being modelled, the W was held down, so Drag could not slow the Player.
            entity.TerminalVelocity = 78.4f;
            physics.AddEntity(entity);

            //
            // Act
            //
            Console.Error.WriteLine($"Before: {entity.Position}");
            physics.Update(TimeSpan.FromSeconds(1));
            Console.Error.WriteLine($"After: {entity.Position}");

            //
            // Assertions
            //
            // x Position should put the entity in contact with the block
            Assert.AreEqual(-4 - entity.Size.Width / 2, entity.Position.X);
            // y position should be unchanged.
            Assert.AreEqual(yPos, entity.Position.Y);
            // z position should be advanced by one time unit's velocity
            Assert.AreEqual(zPos + zVel, entity.Position.Z);

            // x-velocity should be what was required to make contact in one unit
            // of time
            Assert.AreEqual(-4 - xPos + entity.Size.Width / 2, entity.Velocity.X);
            // y-velocity should be unchanged.
            Assert.AreEqual(yVel, entity.Velocity.Y);
            // z-velocity should be unchanged.
            Assert.AreEqual(zVel, entity.Velocity.Z);

            // A collision should have been recordd.
            Assert.True(entity.CollisionOccured);

            //
            // Act again.
            //
            physics.Update(TimeSpan.FromSeconds(1));

            //
            // More assertions
            //
            // x position should still be in contact with the block
            Assert.AreEqual(-4 + entity.Size.Width / 2, entity.Position.X);
            // y position should remain unchanged.
            Assert.AreEqual(yPos, entity.Position.Y);
            // z Position should have advanced by 2 units of time
            Assert.AreEqual(zPos + 2 * zVel, entity.Position.Z);

            // X velocity should now be zero
            Assert.AreEqual(0, entity.Velocity.X);
            // Y velocity should remain unchanged
            Assert.AreEqual(yVel, entity.Velocity.Y);
            // Z velocity should remain unchanged.
            Assert.AreEqual(zVel, entity.Velocity.Z);
        }

        // Testing that when PhysicsEngine.Update is called with
        // different amounts of time, different distances are travelled.
        [TestCase(0.05)]
        [TestCase(0.10)]
        [TestCase(1.00)]
        public void TestVelocity(double seconds)
        {
            double xPos = 8, yPos = 64, zPos = 8;
            double xVel = 1, yVel = 2, zVel = 3;

            TestEntity entity = new();
            entity.Position = new Vector3(xPos, yPos, zPos);
            entity.Velocity = new Vector3(xVel, yVel, zVel);
            entity.AccelerationDueToGravity = 0;
            entity.Drag = 0;

            IDimension dimension = BuildDimension();
            IPhysicsEngine physics = new PhysicsEngine(dimension);

            physics.AddEntity(entity);

            //
            // Act
            //
            physics.Update(TimeSpan.FromSeconds(seconds));

            //
            // Assertions
            //
            // Velocity should be unchanged.
            Assert.AreEqual(new Vector3(xVel, yVel, zVel), entity.Velocity);
            // Position should be updated per units of time given
            Vector3 expectedPosition = new Vector3(xPos + seconds * xVel,
                yPos + seconds * yVel, zPos + seconds * zVel);
            Assert.AreEqual(expectedPosition, entity.Position);
        }
    }
}
