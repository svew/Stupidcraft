using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TrueCraft.Core.World;
using TrueCraft.World;

namespace TrueCraft.Test.World
{
    [TestFixture]
    public class RegionTest
    {
        private Region? _region;

        [OneTimeSetUp]
        public void SetUp()
        {
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            _region = new Region(RegionCoordinates.Zero, 
                Path.Combine(assemblyDir, "Files"));
        }

        [Test]
        public void TestGetChunk()
        {
            IChunk? chunk = _region!.GetChunk(LocalChunkCoordinates.Zero);

            // No chunk was added, and Region must not generate chunks.
            Assert.IsNull(chunk);
        }

        [Test]
        public void TestGetRegionFileName()
        {
            Assert.AreEqual("r.0.0.mcr", Region.GetRegionFileName(_region!.Position));
        }
    }
}