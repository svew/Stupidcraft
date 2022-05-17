using System;
using System.IO;
using System.Reflection;
using Moq;
using NUnit.Framework;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;
using TrueCraft.World;

namespace TrueCraft.Test.World
{
    [TestFixture]
    public class WorldTest
    {
        [Test]
        public void TestManifestLoaded()
        {
            Mock<IMultiplayerServer> mockServer = new Mock<IMultiplayerServer>(MockBehavior.Strict);

            Mock<IBlockRepository> mockBlockRepository = new Mock<IBlockRepository>(MockBehavior.Strict);

            Mock<IItemRepository> mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);

            Mock<IServiceLocator> mockServiceLocator = new Mock<IServiceLocator>(MockBehavior.Strict);
            mockServiceLocator.Setup(x => x.Server).Returns(mockServer.Object);
            mockServiceLocator.Setup(x => x.BlockRepository).Returns(mockBlockRepository.Object);
            mockServiceLocator.Setup(x => x.ItemRepository).Returns(mockItemRepository.Object);

            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            IWorld world = TrueCraft.World.World.LoadWorld(mockServiceLocator.Object, Path.Combine(assemblyDir, "Files"));

            // Constants from manifest.nbt
            Assert.AreEqual(new PanDimensionalVoxelCoordinates(DimensionID.Overworld, 0, 60, 0), world.SpawnPoint);
            Assert.AreEqual(1168393583, world.Seed);
        }
    }
}