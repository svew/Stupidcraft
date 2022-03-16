using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Test.World
{
    [TestFixture]
    public class RegionTest
    {
        public Region Region { get; set; }

        [OneTimeSetUp]
        public void SetUp()
        {
            IDimension dimension = new TrueCraft.Core.World.Dimension();
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Region = new Region(RegionCoordinates.Zero, dimension,
                Path.Combine(assemblyDir, "Files", "r.0.0.mca"));
        }

        [Test]
        public void TestGetChunk()
        {
            var chunk = Region.GetChunk(LocalChunkCoordinates.Zero);
            Assert.AreEqual(GlobalChunkCoordinates.Zero, chunk.Coordinates);
            Assert.Throws(typeof(ArgumentException), () =>
                Region.GetChunk(new LocalChunkCoordinates(31, 31)));
        }

        [Test]
        public void TestUnloadChunk()
        {
            var chunk = Region.GetChunk(LocalChunkCoordinates.Zero);
            Assert.AreEqual(GlobalChunkCoordinates.Zero, chunk.Coordinates);
            Assert.IsTrue(Region.Chunks.Any(c => c.Coordinates == GlobalChunkCoordinates.Zero));
            Region.UnloadChunk(LocalChunkCoordinates.Zero);
            Assert.IsFalse(Region.Chunks.Any(c => c.Coordinates == GlobalChunkCoordinates.Zero));
        }

        [Test]
        public void TestGetRegionFileName()
        {
            Assert.AreEqual("r.0.0.mca", Region.GetRegionFileName(Region.Position));
        }
    }
}