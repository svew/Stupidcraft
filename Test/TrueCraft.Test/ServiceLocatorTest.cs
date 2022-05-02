using System;
using Moq;
using NUnit.Framework;
using TrueCraft;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;
using TrueCraft.World;

namespace TrueCraft.Test
{
    public class ServiceLocatorTest
    {
        public ServiceLocatorTest()
        {
        }

        [Test]
        public void ctor()
        {
            Mock<IMultiplayerServer> mockServer = new Mock<IMultiplayerServer>(MockBehavior.Strict);
            Mock<IBlockRepository> mockBlockRepository = new Mock<IBlockRepository>(MockBehavior.Strict);
            Mock<IItemRepository> mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);

            IServiceLocator locator = new ServiceLocater(mockServer.Object,
                mockBlockRepository.Object,
                mockItemRepository.Object);

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

            Assert.Throws<ArgumentNullException>(() => new ServiceLocater(null!, mockBlockRepository.Object, mockItemRepository.Object));
            Assert.Throws<ArgumentNullException>(() => new ServiceLocater(mockServer.Object, null!, mockItemRepository.Object));
            Assert.Throws<ArgumentNullException>(() => new ServiceLocater(mockServer.Object, mockBlockRepository.Object, null!));
        }

        [Test]
        public void WorldSetter()
        {
            Mock<IMultiplayerServer> mockServer = new Mock<IMultiplayerServer>(MockBehavior.Strict);
            Mock<IBlockRepository> mockBlockRepository = new Mock<IBlockRepository>(MockBehavior.Strict);
            Mock<IItemRepository> mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);

            IServiceLocator locator = new ServiceLocater(mockServer.Object,
                mockBlockRepository.Object,
                mockItemRepository.Object);

            Mock<IWorld> mockWorld = new Mock<IWorld>(MockBehavior.Strict);

            Assert.Throws<InvalidOperationException>(() => { IWorld t = locator.World; });
            locator.World = mockWorld.Object;
            Assert.Throws<InvalidOperationException>(() => locator.World = mockWorld.Object);
            Assert.True(object.ReferenceEquals(mockWorld.Object, locator.World));
        }
    }
}
