using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using TrueCraft.Core.TerrainGen;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Test.World
{
    [TestFixture]
    public class DimensionTest
    {
        private IDimension _dimension;

        [OneTimeSetUp]
        public void SetUp()
        {
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _dimension = new Dimension(assemblyDir, "default");
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