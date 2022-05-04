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
        private readonly string _assemblyDir;

        private readonly IServiceLocator _serviceLocator;

        private readonly ILightingQueue _lightingQueue;

        private readonly IEntityManager _entityManager;

        public DimensionTest()
        {
            Mock<IBlockProvider> mockProvider = new Mock<IBlockProvider>(MockBehavior.Strict);
            mockProvider.Setup(x => x.ID).Returns(3);

            Mock<IBlockRepository> mockRepository = new Mock<IBlockRepository>(MockBehavior.Strict);
            mockRepository.Setup(x => x.GetBlockProvider(It.Is<byte>(b => b == 3))).Returns(mockProvider.Object);

            Mock<IMultiplayerServer> mockServer = new Mock<IMultiplayerServer>(MockBehavior.Strict);

            Mock<IServiceLocator> mockServiceLocator = new Mock<IServiceLocator>(MockBehavior.Strict);
            mockServiceLocator.Setup(x => x.BlockRepository).Returns(mockRepository.Object);
            mockServiceLocator.Setup(x => x.Server).Returns(mockServer.Object);
            _serviceLocator = mockServiceLocator.Object;

            Mock<ILightingQueue> mockLightingQueue = new Mock<ILightingQueue>(MockBehavior.Strict);
            _lightingQueue = mockLightingQueue.Object;

            Mock<IEntityManager> mockEntityManager = new Mock<IEntityManager>(MockBehavior.Strict);
            _entityManager = mockEntityManager.Object;

            _assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        }

        private IDimensionServer BuildDimension()
        {
            return new Dimension(_serviceLocator, _assemblyDir, DimensionID.Overworld,
                new FlatlandGenerator(1234), _lightingQueue, _entityManager);
        }

        /// <summary>
        /// Tests that the GetChunk(GlobalVoxelCoordinates) method does not
        /// generate a Chunk that is not already loaded.
        /// </summary>
        [Test]
        public void TestGetChunk_Global_NoGenerate()
        {
            IDimension dimension = BuildDimension();

            IChunk? a = dimension.GetChunk(new GlobalVoxelCoordinates(0, 0, 0));
            IChunk? b = dimension.GetChunk(new GlobalVoxelCoordinates(-1, 0, 0));
            IChunk? c = dimension.GetChunk(new GlobalVoxelCoordinates(-1, 0, -1));
            IChunk? d = dimension.GetChunk(new GlobalVoxelCoordinates(0, 0, -1));

            Assert.IsNull(a);
            Assert.IsNull(b);
            Assert.IsNull(c);
            Assert.IsNull(d);
        }

        /// <summary>
        /// Tests that the GetChunk(GlobalChunkCoordinates) method does not
        /// generate a Chunk that is not already loaded.
        /// </summary>
        [Test]
        public void TestGetChunk_Chunk_NoGenerate()
        {
            IDimension dimension = BuildDimension();

            IChunk? a = dimension.GetChunk(new GlobalChunkCoordinates(0, 0));
            IChunk? b = dimension.GetChunk(new GlobalChunkCoordinates(-1, 0));
            IChunk? c = dimension.GetChunk(new GlobalChunkCoordinates(-1, -1));
            IChunk? d = dimension.GetChunk(new GlobalChunkCoordinates(0, -1));

            Assert.IsNull(a);
            Assert.IsNull(b);
            Assert.IsNull(c);
            Assert.IsNull(d);
        }
    }
}