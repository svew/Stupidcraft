using System;
using Moq;
using NUnit.Framework;
using TrueCraft;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;
using TrueCraft.World;

namespace TrueCraft.Test
{
    public class ServerServiceLocatorTest
    {
        public ServerServiceLocatorTest()
        {
        }

        [Test]
        public void ctor()
        {
            Mock<IMultiplayerServer> mockServer = new Mock<IMultiplayerServer>(MockBehavior.Strict);
            Mock<IBlockRepository> mockBlockRepository = new Mock<IBlockRepository>(MockBehavior.Strict);
            Mock<IItemRepository> mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);
            Mock<IServiceLocator> mockServiceLocator = new Mock<IServiceLocator>(MockBehavior.Strict);
            mockServiceLocator.Setup(x => x.ItemRepository).Returns(mockItemRepository.Object);
            mockServiceLocator.Setup(x => x.BlockRepository).Returns(mockBlockRepository.Object);

            IServerServiceLocator locator = new ServerServiceLocator(mockServer.Object,
                mockServiceLocator.Object);

            Assert.Throws<InvalidOperationException>(() => { IWorld t = locator.World; });
            Assert.True(object.ReferenceEquals(mockServer.Object, locator.Server));
            Assert.True(object.ReferenceEquals(mockBlockRepository.Object, locator.BlockRepository));
            Assert.True(object.ReferenceEquals(mockItemRepository.Object, locator.ItemRepository));
        }

        [Test]
        public void ctor_Throws()
        {
            Mock<IMultiplayerServer> mockServer = new Mock<IMultiplayerServer>(MockBehavior.Strict);
            Mock<IBlockRepository> mockBlockRepository = new Mock<IBlockRepository>(MockBehavior.Strict);
            Mock<IItemRepository> mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);
            Mock<IServiceLocator> mockServiceLocator = new Mock<IServiceLocator>(MockBehavior.Strict);
            mockServiceLocator.Setup(x => x.ItemRepository).Returns(mockItemRepository.Object);
            mockServiceLocator.Setup(x => x.BlockRepository).Returns(mockBlockRepository.Object);

            Assert.Throws<ArgumentNullException>(() => new ServerServiceLocator(null!, mockServiceLocator.Object));
        }

        [Test]
        public void WorldSetter()
        {
            Mock<IMultiplayerServer> mockServer = new Mock<IMultiplayerServer>(MockBehavior.Strict);
            Mock<IBlockRepository> mockBlockRepository = new Mock<IBlockRepository>(MockBehavior.Strict);
            Mock<IItemRepository> mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);
            Mock<IServiceLocator> mockServiceLocator = new Mock<IServiceLocator>(MockBehavior.Strict);
            mockServiceLocator.Setup(x => x.ItemRepository).Returns(mockItemRepository.Object);
            mockServiceLocator.Setup(x => x.BlockRepository).Returns(mockBlockRepository.Object);

            IServerServiceLocator locator = new ServerServiceLocator(mockServer.Object,
                mockServiceLocator.Object);

            Mock<IWorld> mockWorld = new Mock<IWorld>(MockBehavior.Strict);

            Assert.Throws<InvalidOperationException>(() => { IWorld t = locator.World; });
            locator.World = mockWorld.Object;
            Assert.Throws<InvalidOperationException>(() => locator.World = mockWorld.Object);
            Assert.True(object.ReferenceEquals(mockWorld.Object, locator.World));
        }
    }
}
