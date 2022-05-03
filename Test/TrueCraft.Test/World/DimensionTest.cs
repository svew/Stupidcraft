using System;
using System.IO;
using System.Reflection;
using Moq;
using NUnit.Framework;
using TrueCraft.Core.Lighting;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;
using TrueCraft.TerrainGen;
using TrueCraft.World;

namespace TrueCraft.Test.World
{
    [TestFixture]
    public class DimensionTest
    {
        private IDimension _dimension;

        [OneTimeSetUp]
        public void SetUp()
        {
            Mock<IBlockProvider> mockProvider = new Mock<IBlockProvider>(MockBehavior.Strict);
            mockProvider.Setup(x => x.ID).Returns(3);

            Mock<IBlockRepository> mockRepository = new Mock<IBlockRepository>(MockBehavior.Strict);
            mockRepository.Setup(x => x.GetBlockProvider(It.Is<byte>(b => b == 3))).Returns(mockProvider.Object);

            Mock<IMultiplayerServer> mockServer = new Mock<IMultiplayerServer>(MockBehavior.Strict);

            Mock<IServiceLocator> mockServiceLocator = new Mock<IServiceLocator>(MockBehavior.Strict);
            mockServiceLocator.Setup(x => x.BlockRepository).Returns(mockRepository.Object);
            mockServiceLocator.Setup(x => x.Server).Returns(mockServer.Object);

            Mock<ILightingQueue> mockLightingQueue = new Mock<ILightingQueue>(MockBehavior.Strict);

            Mock<IEntityManager> mockEntityManager = new Mock<IEntityManager>(MockBehavior.Strict);

            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _dimension = new Dimension(mockServiceLocator.Object,
                assemblyDir, DimensionID.Overworld, 
                new FlatlandGenerator(1234), mockLightingQueue.Object,
                mockEntityManager.Object);
        }

        [Test]
        public void TestFindChunk()
        {
            var a = _dimension.GetChunk(new GlobalVoxelCoordinates(0, 0, 0));
            var b = _dimension.GetChunk(new GlobalVoxelCoordinates(-1, 0, 0));
            var c = _dimension.GetChunk(new GlobalVoxelCoordinates(-1, 0, -1));
            var d = _dimension.GetChunk(new GlobalVoxelCoordinates(16, 0, 0));
            Assert.AreEqual(new GlobalChunkCoordinates(0, 0), a.Coordinates);
            Assert.AreEqual(new GlobalChunkCoordinates(-1, 0), b.Coordinates);
            Assert.AreEqual(new GlobalChunkCoordinates(-1, -1), c.Coordinates);
            Assert.AreEqual(new GlobalChunkCoordinates(1, 0), d.Coordinates);
        }

        [Test]
        public void TestGetChunk()
        {
            var a = _dimension.GetChunk(new GlobalChunkCoordinates(0, 0));
            var b = _dimension.GetChunk(new GlobalChunkCoordinates(1, 0));
            var c = _dimension.GetChunk(new GlobalChunkCoordinates(-1, 0));
            Assert.AreEqual(new GlobalChunkCoordinates(0, 0), a.Coordinates);
            Assert.AreEqual(new GlobalChunkCoordinates(1, 0), b.Coordinates);
            Assert.AreEqual(new GlobalChunkCoordinates(-1, 0), c.Coordinates);
        }
    }
}