using System;
using NUnit.Framework;
using Moq;
using Moq.Protected;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;
using TrueCraft.Core;
using TrueCraft.TerrainGen;
using TrueCraft.Core.Lighting;

namespace TrueCraft.Test.Logic
{
    // TODO: This unit test depends upon the FlatlandGenerator
    [TestFixture]
    public class BlockProviderTest
    {
        private const int _testSeed = 314159;

        private readonly Mock<IDimensionServer> _dimension;
        private readonly Mock<IMultiplayerServer> _server;
        private readonly Mock<IEntityManager> _entityManager;
        private readonly Mock<IRemoteClient> _user;
        private readonly Mock<IBlockRepository> _blockRepository;

        public BlockProviderTest()
        {
            _dimension = new Mock<IDimensionServer>();
            _server = new Mock<IMultiplayerServer>();
            _entityManager = new Mock<IEntityManager>();
            _user = new Mock<IRemoteClient>();
            _blockRepository = new Mock<IBlockRepository>();

            _user.SetupGet(u => u.Dimension).Returns(_dimension.Object);
            _user.SetupGet(u => u.Server).Returns(_server.Object);

            _dimension.Setup(w => w.SetBlockID(It.IsAny<GlobalVoxelCoordinates>(), It.IsAny<byte>()));
            _dimension.Setup(d => d.BlockRepository).Returns(_blockRepository.Object);

            _server.SetupGet(s => s.BlockRepository).Returns(_blockRepository.Object);

            _entityManager.Setup(m => m.SpawnEntity(It.IsAny<IEntity>()));
        }

        protected void ResetMocks()
        {
            _dimension.Invocations.Clear();
            _server.Invocations.Clear();
            _entityManager.Invocations.Clear();
            _user.Invocations.Clear();
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            try
            {
                WhoAmI.Answer = IAm.Server;
            }
            catch (InvalidOperationException)
            {
                // Ignore this - it just means we've tried to set WhoAmI
                // multiple times.
            }
        }

        [Test]
        public void TestBlockMined()
        {
            //
            // Set up
            //
            bool generateDropEntityCalled = false;
            BlockDescriptor? generateDescriptor = null;
            IDimension? generateDimension = null;
            IMultiplayerServer? generateServer = null;
            ItemStack? generateHeldItem = null;
            Mock<BlockProvider> blockProvider = new Mock<BlockProvider>(MockBehavior.Strict);
            blockProvider.Setup(x => x.BlockMined(It.IsAny<BlockDescriptor>(),
                It.IsAny<BlockFace>(), It.IsAny<IDimension>(), It.IsAny<IRemoteClient>())).CallBase();
            blockProvider.Setup(x => x.GenerateDropEntity(It.IsAny<BlockDescriptor>(),
                It.IsAny<IDimension>(), It.IsAny<IMultiplayerServer>(), It.IsAny<ItemStack>()))
                .Callback<BlockDescriptor, IDimension, IMultiplayerServer, ItemStack>(
                (block, dimension, server, heldItem) =>
                {
                    generateDropEntityCalled = true;
                    generateDescriptor = block;
                    generateDimension = dimension;
                    generateServer = server;
                    generateHeldItem = heldItem;
                });

            // NOTE: dependency upon BlockDescriptor and GlobalVoxelCoordinates.
            GlobalVoxelCoordinates blockCoordinates = new GlobalVoxelCoordinates(5, 15, 10);
            var descriptor = new BlockDescriptor
            {
                ID = 10,
                Coordinates = blockCoordinates
            };

            Mock<IEntityManager> mockEntityManager = new Mock<IEntityManager>(MockBehavior.Strict);

            Mock<IItemRepository> mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);

            GlobalVoxelCoordinates blockSetAt = GlobalVoxelCoordinates.Zero;
            byte newBlockID = 1;
            Mock<IDimensionServer> mockDimension = new Mock<IDimensionServer>(MockBehavior.Strict);
            mockDimension.Setup(x => x.EntityManager).Returns(mockEntityManager.Object);
            mockDimension.Setup(x => x.ItemRepository).Returns(mockItemRepository.Object);
            mockDimension.Setup(x => x.SetBlockID(It.IsAny<GlobalVoxelCoordinates>(), It.IsAny<byte>()))
                .Callback<GlobalVoxelCoordinates, byte>((coord, id) => { blockSetAt = coord; newBlockID = id; });

            Mock<IMultiplayerServer> mockServer = new Mock<IMultiplayerServer>(MockBehavior.Strict);

            Mock<IRemoteClient> mockRemoteClient = new Mock<IRemoteClient>(MockBehavior.Strict);
            mockRemoteClient.Setup(x => x.Server).Returns(mockServer.Object);
            // NOTE: dependent upon ItemStack class.
            mockRemoteClient.Setup(x => x.SelectedItem).Returns(ItemStack.EmptyStack);

            //
            // Act
            //
            blockProvider.Object.BlockMined(descriptor, BlockFace.PositiveY, mockDimension.Object, mockRemoteClient.Object);

            //
            // Assert
            //
            Assert.AreEqual(blockCoordinates, blockSetAt);
            Assert.AreEqual(0, newBlockID);
            Assert.True(generateDropEntityCalled);
            Assert.IsNotNull(generateDescriptor);
            Assert.AreEqual(10, generateDescriptor?.ID);
            Assert.AreEqual(blockCoordinates, generateDescriptor?.Coordinates);
            Assert.True(object.ReferenceEquals(mockDimension.Object, generateDimension));
            Assert.AreEqual(ItemStack.EmptyStack, generateHeldItem);
            Assert.True(object.ReferenceEquals(mockServer.Object, generateServer));
        }

        [Test]
        public void TestSupport()
        {
            ResetMocks();

            // We need an actual world for this
            // TODO: Check if this could be switched to FakeDimension so that this unit test won't have
            //       a dependency on the Dimension class.
            Mock<IMultiplayerServer> mockServer = new Mock<IMultiplayerServer>(MockBehavior.Strict);
            Mock<IServiceLocator> mockServiceLocator = new Mock<IServiceLocator>(MockBehavior.Strict);
            mockServiceLocator.Setup(x => x.Server).Returns(mockServer.Object);
            Mock<ILightingQueue> mockLightingQueue = new Mock<ILightingQueue>(MockBehavior.Strict);
            IDimension dimension = new TrueCraft.World.Dimension(mockServiceLocator.Object, string.Empty,
                      DimensionID.Overworld, new FlatlandGenerator(_testSeed),
                      mockLightingQueue.Object,
                      _entityManager.Object);

            dimension.SetBlockID(GlobalVoxelCoordinates.Zero, 1);
            GlobalVoxelCoordinates oneY = new GlobalVoxelCoordinates(0, 1, 0);
            dimension.SetBlockID(oneY, 2);

            var blockProvider = new Mock<BlockProvider> { CallBase = true };
            var updated = new BlockDescriptor { ID = 2, Coordinates = oneY };
            var source = new BlockDescriptor { ID = 2, Coordinates = new GlobalVoxelCoordinates(1, 0, 0) };
            blockProvider.Setup(b => b.GetSupportDirection(It.IsAny<BlockDescriptor>())).Returns(Vector3i.Down);

            var supportive = new Mock<IBlockProvider>();
            supportive.SetupGet(p => p.Opaque).Returns(true);
            var unsupportive = new Mock<IBlockProvider>();
            unsupportive.SetupGet(p => p.Opaque).Returns(false);

            _blockRepository.Setup(r => r.GetBlockProvider(It.Is<byte>(b => b == 1))).Returns(supportive.Object);
            _blockRepository.Setup(r => r.GetBlockProvider(It.Is<byte>(b => b == 3))).Returns(unsupportive.Object);

            blockProvider.Object.BlockUpdate(updated, source, _server.Object, dimension);
            _dimension.Verify(w => w.SetBlockID(oneY, 0), Times.Never);

            dimension.SetBlockID(GlobalVoxelCoordinates.Zero, 3);

            blockProvider.Object.BlockUpdate(updated, source, _server.Object, dimension);
            Assert.AreEqual(0, dimension.GetBlockID(oneY));
            _entityManager.Verify(m => m.SpawnEntity(It.Is<ItemEntity>(e => e.Item.ID == 2)));
        }
    }
}