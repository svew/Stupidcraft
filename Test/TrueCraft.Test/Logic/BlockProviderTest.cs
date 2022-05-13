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
using TrueCraft.Test.World;
using TrueCraft.Core.Logic.Items;

namespace TrueCraft.Test.Logic
{
    // TODO: This unit test depends upon the FlatlandGenerator
    [TestFixture]
    public class BlockProviderTest
    {
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
            //
            // Setup
            //
            Mock<IBlockProvider> supportive = new Mock<IBlockProvider>(MockBehavior.Strict);
            supportive.SetupGet(p => p.Opaque).Returns(true);
            supportive.SetupGet<byte>(p => p.ID).Returns(1);
            Mock<IBlockProvider> needsSupport = new Mock<IBlockProvider>(MockBehavior.Strict);
            needsSupport.SetupGet<byte>(p => p.ID).Returns(2);
            Mock<IBlockProvider> unsupportive = new Mock<IBlockProvider>(MockBehavior.Strict);
            unsupportive.SetupGet(p => p.Opaque).Returns(false);
            unsupportive.SetupGet<byte>(p => p.ID).Returns(3);

            Mock<IBlockRepository> mockBlockRepository = new Mock<IBlockRepository>(MockBehavior.Strict);
            mockBlockRepository.Setup(r => r.GetBlockProvider(It.Is<byte>(b => b == supportive.Object.ID))).Returns(supportive.Object);
            mockBlockRepository.Setup(r => r.GetBlockProvider(It.Is<byte>(b => b == unsupportive.Object.ID))).Returns(unsupportive.Object);

            Mock<IItemRepository> mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);
            mockItemRepository.Setup(x => x.GetItemProvider(It.Is<short>(x => x == -1))).Returns<IItemProvider>(null);

            Mock<IMultiplayerServer> mockServer = new Mock<IMultiplayerServer>(MockBehavior.Strict);

            Mock<IServiceLocator> mockServiceLocator = new Mock<IServiceLocator>(MockBehavior.Strict);
            mockServiceLocator.Setup(x => x.Server).Returns(mockServer.Object);
            mockServiceLocator.Setup(x => x.BlockRepository).Returns(mockBlockRepository.Object);
            mockServiceLocator.Setup(x => x.ItemRepository).Returns(mockItemRepository.Object);

            Mock<IEntityManager> mockEntityManager = new Mock<IEntityManager>(MockBehavior.Strict);
            IEntity? spawnedEntity = null;
            mockEntityManager.Setup(x => x.SpawnEntity(It.IsAny<IEntity>())).Callback<IEntity>(
                (entity) =>
                {
                    spawnedEntity = entity;
                });

            IDimension fakeDimension = new FakeDimension(mockBlockRepository.Object,
                mockItemRepository.Object, mockEntityManager.Object);

            fakeDimension.SetBlockID(GlobalVoxelCoordinates.Zero, supportive.Object.ID);
            GlobalVoxelCoordinates oneY = new GlobalVoxelCoordinates(0, 1, 0);
            fakeDimension.SetBlockID(oneY, needsSupport.Object.ID);

            // Note: It would be preferable to have this be a Strict Mock rather
            //  than a loose one, but the setup of GetDrop is not working.
            Mock<BlockProvider> testBlockProvider = new Mock<BlockProvider>(MockBehavior.Loose);
            testBlockProvider.CallBase = true;
            testBlockProvider.SetupGet<byte>(x => x.ID).Returns(2);
            testBlockProvider.SetupGet<ToolType>(x => x.EffectiveTools).Returns(ToolType.All);
            testBlockProvider.SetupGet<ToolMaterial>(x => x.EffectiveToolMaterials).Returns(ToolMaterial.All);
            testBlockProvider.Setup(b => b.GetSupportDirection(It.IsAny<BlockDescriptor>())).Returns(Vector3i.Down);
            testBlockProvider.Setup(x => x.BlockUpdate(It.IsAny<BlockDescriptor>(),
                It.IsAny<BlockDescriptor>(), It.IsAny<IMultiplayerServer>(),
                It.IsAny<IDimension>())).CallBase();
            testBlockProvider.Setup(x => x.IsSupported(It.IsAny<IDimension>(),
                It.IsAny<BlockDescriptor>())).CallBase();
            testBlockProvider.Setup(x => x.GenerateDropEntity(It.IsAny<BlockDescriptor>(),
                It.IsAny<IDimension>(), It.IsAny<IMultiplayerServer>(), It.IsAny<ItemStack>()))
                .CallBase();
            // Note: This could not be made to work.  It was throwing an exception
            // stating that
            // "all invocations on the Mock must have a corresponding setup"
            // Changing the passed in signature produced an error as expected so
            // the setup was able to find the method, but execution of the Mock
            // was not.
            //testBlockProvider.Protected().Setup<ItemStack[]>("GetDrop", It.IsAny<BlockDescriptor>(), It.IsAny<ItemStack>()).CallBase();

            BlockDescriptor updated = new BlockDescriptor { ID = needsSupport.Object.ID, Coordinates = oneY };
            BlockDescriptor source = new BlockDescriptor { ID = needsSupport.Object.ID, Coordinates = new GlobalVoxelCoordinates(1, 0, 0) };


            //
            // Act / Assert
            //

            // Send the block an update from the side
            testBlockProvider.Object.BlockUpdate(updated, source, mockServer.Object, fakeDimension);
            // Assert that the block needing support is still there.
            Assert.AreEqual(needsSupport.Object.ID, fakeDimension.GetBlockID(oneY));

            // Switch the block underneath to one that does not provide support.
            fakeDimension.SetBlockID(GlobalVoxelCoordinates.Zero, unsupportive.Object.ID);
            // Send the supported block an update from below
            source = new BlockDescriptor { ID = unsupportive.Object.ID, Coordinates = GlobalVoxelCoordinates.Zero };
            testBlockProvider.Object.BlockUpdate(updated, source, mockServer.Object, fakeDimension);
            // Assert that the block requiring support has been replace with an Air Block
            Assert.AreEqual(0, fakeDimension.GetBlockID(oneY));
            // Assert that an Item Entity with ID == 2 was spawned.
            Assert.IsNotNull(spawnedEntity);
            ItemEntity? itemEntity = spawnedEntity as ItemEntity;
            Assert.IsNotNull(itemEntity);
            Assert.AreEqual(needsSupport.Object.ID, itemEntity?.Item.ID);
        }

        /// <summary>
        /// Test GenerateDropEntity using a tool which is effective and of the correct material
        /// </summary>
        [Test]
        public void GenerateDropEntity_Effective()
        {
            //
            // Setup
            //
            GlobalVoxelCoordinates coordinates = new GlobalVoxelCoordinates(5, 7, 13);
            byte blockID = 47;
            BlockDescriptor block = new BlockDescriptor()
            {
                ID = blockID,
                Coordinates = coordinates
            };

            Mock<BlockProvider> testBlockProvider = new Mock<BlockProvider>(MockBehavior.Loose);
            testBlockProvider.CallBase = true;
            testBlockProvider.Setup(x => x.EffectiveToolMaterials).CallBase();
            testBlockProvider.Setup(x => x.EffectiveTools).CallBase();
            testBlockProvider.Setup(x => x.GenerateDropEntity(It.IsAny<BlockDescriptor>(),
                It.IsAny<IDimension>(), It.IsAny<IMultiplayerServer>(), It.IsAny<ItemStack>()))
                .CallBase();

            Mock<IMultiplayerServer> mockServer = new Mock<IMultiplayerServer>(MockBehavior.Strict);

            short toolItemID = 12;
            ItemStack heldItem = new ItemStack(toolItemID);

            Mock<ToolItem> mockTool = new Mock<ToolItem>(MockBehavior.Strict);
            mockTool.Setup(x => x.Material).Returns(ToolMaterial.Stone);
            mockTool.Setup(x => x.ToolType).Returns(ToolType.Hoe);

            Mock<IItemRepository> mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);
            mockItemRepository.Setup(x => x.GetItemProvider(It.Is<short>(y => y == toolItemID))).Returns(mockTool.Object);

            Mock<IEntityManager> mockEntityManager = new Mock<IEntityManager>(MockBehavior.Strict);
            int spawnCount = 0;
            IEntity? spawnedEntity = null;
            mockEntityManager.Setup(x => x.SpawnEntity(It.IsAny<IEntity>())).Callback<IEntity>((entity) =>
            {
                spawnCount++;
                spawnedEntity = entity;
            });

            Mock<IDimensionServer> mockDimension = new Mock<IDimensionServer>(MockBehavior.Strict);
            mockDimension.Setup(x => x.ItemRepository).Returns(mockItemRepository.Object);
            mockDimension.Setup(x => x.EntityManager).Returns(mockEntityManager.Object);

            //
            // Act
            //
            testBlockProvider.Object.GenerateDropEntity(block, mockDimension.Object,
                mockServer.Object, heldItem);

            //
            // Assert
            //
            Assert.AreEqual(1, spawnCount);
            ItemEntity? spawnedItem = spawnedEntity as ItemEntity;
            Assert.IsNotNull(spawnedItem);
            Assert.AreEqual(blockID, spawnedItem?.Item.ID);
            Assert.AreEqual(1, spawnedItem?.Item.Count);
            Assert.AreEqual(coordinates.X, Math.Floor(spawnedEntity?.Position.X ?? double.MinValue));
            Assert.AreEqual(coordinates.Y, Math.Floor(spawnedEntity?.Position.Y ?? double.MinValue));
            Assert.AreEqual(coordinates.Z, Math.Floor(spawnedEntity?.Position.Z ?? double.MinValue));
        }

        /// <summary>
        /// Test GenerateDropEntity using a tool which is ineffective but of the correct material
        /// </summary>
        [Test]
        public void GenerateDropEntity_IneffectiveTool()
        {
            //
            // Setup
            //
            GlobalVoxelCoordinates coordinates = new GlobalVoxelCoordinates(5, 7, 13);
            byte blockID = 47;
            BlockDescriptor block = new BlockDescriptor()
            {
                ID = blockID,
                Coordinates = coordinates
            };

            Mock<BlockProvider> testBlockProvider = new Mock<BlockProvider>(MockBehavior.Loose);
            testBlockProvider.CallBase = true;
            testBlockProvider.Setup(x => x.EffectiveToolMaterials).Returns(ToolMaterial.Diamond);
            testBlockProvider.Setup(x => x.EffectiveTools).Returns(ToolType.Pickaxe);
            testBlockProvider.Setup(x => x.GenerateDropEntity(It.IsAny<BlockDescriptor>(),
                It.IsAny<IDimension>(), It.IsAny<IMultiplayerServer>(), It.IsAny<ItemStack>()))
                .CallBase();

            Mock<IMultiplayerServer> mockServer = new Mock<IMultiplayerServer>(MockBehavior.Strict);

            short toolItemID = 12;
            ItemStack heldItem = new ItemStack(toolItemID);

            Mock<ToolItem> mockTool = new Mock<ToolItem>(MockBehavior.Strict);
            mockTool.Setup(x => x.Material).Returns(ToolMaterial.Diamond);
            mockTool.Setup(x => x.ToolType).Returns(ToolType.Hoe);

            Mock<IItemRepository> mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);
            mockItemRepository.Setup(x => x.GetItemProvider(It.Is<short>(y => y == toolItemID))).Returns(mockTool.Object);

            Mock<IEntityManager> mockEntityManager = new Mock<IEntityManager>(MockBehavior.Strict);
            int spawnCount = 0;
            IEntity? spawnedEntity = null;
            mockEntityManager.Setup(x => x.SpawnEntity(It.IsAny<IEntity>())).Callback<IEntity>((entity) =>
            {
                spawnCount++;
                spawnedEntity = entity;
            });

            Mock<IDimensionServer> mockDimension = new Mock<IDimensionServer>(MockBehavior.Strict);
            mockDimension.Setup(x => x.ItemRepository).Returns(mockItemRepository.Object);
            mockDimension.Setup(x => x.EntityManager).Returns(mockEntityManager.Object);

            //
            // Act
            //
            testBlockProvider.Object.GenerateDropEntity(block, mockDimension.Object,
                mockServer.Object, heldItem);

            //
            // Assert
            //
            Assert.AreEqual(0, spawnCount);
            Assert.IsNull(spawnedEntity);
        }

        /// <summary>
        /// Test GenerateDropEntity using a tool which is effective but of the incorrect material
        /// </summary>
        [Test]
        public void GenerateDropEntity_IneffectiveMaterial()
        {
            //
            // Setup
            //
            GlobalVoxelCoordinates coordinates = new GlobalVoxelCoordinates(5, 7, 13);
            byte blockID = 47;
            BlockDescriptor block = new BlockDescriptor()
            {
                ID = blockID,
                Coordinates = coordinates
            };

            Mock<BlockProvider> testBlockProvider = new Mock<BlockProvider>(MockBehavior.Loose);
            testBlockProvider.CallBase = true;
            testBlockProvider.Setup(x => x.EffectiveToolMaterials).Returns(ToolMaterial.Diamond);
            testBlockProvider.Setup(x => x.EffectiveTools).Returns(ToolType.Pickaxe);
            testBlockProvider.Setup(x => x.GenerateDropEntity(It.IsAny<BlockDescriptor>(),
                It.IsAny<IDimension>(), It.IsAny<IMultiplayerServer>(), It.IsAny<ItemStack>()))
                .CallBase();

            Mock<IMultiplayerServer> mockServer = new Mock<IMultiplayerServer>(MockBehavior.Strict);

            short toolItemID = 12;
            ItemStack heldItem = new ItemStack(toolItemID);

            Mock<ToolItem> mockTool = new Mock<ToolItem>(MockBehavior.Strict);
            mockTool.Setup(x => x.Material).Returns(ToolMaterial.Stone);
            mockTool.Setup(x => x.ToolType).Returns(ToolType.Pickaxe);

            Mock<IItemRepository> mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);
            mockItemRepository.Setup(x => x.GetItemProvider(It.Is<short>(y => y == toolItemID))).Returns(mockTool.Object);

            Mock<IEntityManager> mockEntityManager = new Mock<IEntityManager>(MockBehavior.Strict);
            int spawnCount = 0;
            IEntity? spawnedEntity = null;
            mockEntityManager.Setup(x => x.SpawnEntity(It.IsAny<IEntity>())).Callback<IEntity>((entity) =>
            {
                spawnCount++;
                spawnedEntity = entity;
            });

            Mock<IDimensionServer> mockDimension = new Mock<IDimensionServer>(MockBehavior.Strict);
            mockDimension.Setup(x => x.ItemRepository).Returns(mockItemRepository.Object);
            mockDimension.Setup(x => x.EntityManager).Returns(mockEntityManager.Object);

            //
            // Act
            //
            testBlockProvider.Object.GenerateDropEntity(block, mockDimension.Object,
                mockServer.Object, heldItem);

            //
            // Assert
            //
            Assert.AreEqual(0, spawnCount);
            Assert.IsNull(spawnedEntity);
        }
    }
}