using System;
using Moq;
using NUnit.Framework;
using TrueCraft.Core;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;

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
            Mock<IBlockRepository> mockBlockRepository = new Mock<IBlockRepository>(MockBehavior.Strict);
            Mock<IItemRepository> mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);

            IServiceLocator locator = new ServiceLocator(mockBlockRepository.Object,
                mockItemRepository.Object);

            Assert.True(object.ReferenceEquals(mockBlockRepository.Object, locator.BlockRepository));
            Assert.True(object.ReferenceEquals(mockItemRepository.Object, locator.ItemRepository));
        }

        [Test]
        public void ctor_Throws()
        {
            Mock<IBlockRepository> mockBlockRepository = new Mock<IBlockRepository>(MockBehavior.Strict);
            Mock<IItemRepository> mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);

            Assert.Throws<ArgumentNullException>(() => new ServiceLocator(null!, mockItemRepository.Object));
            Assert.Throws<ArgumentNullException>(() => new ServiceLocator(mockBlockRepository.Object, null!));
        }
    }
}
