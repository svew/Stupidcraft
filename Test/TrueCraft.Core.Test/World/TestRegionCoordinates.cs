using System;
using TrueCraft.Core.World;
using NUnit.Framework;

namespace TrueCraft.Core.Test.World
{
    [TestFixture]
    public class TestRegionCoordinates
    {
        private Random _random;

        [OneTimeSetUp]
        public void SetUp()
        {
            _random = new Random(1234);
        }

        [TestCase(2, 3)]
        public void ctor(int x, int z)
        {
            RegionCoordinates a = new RegionCoordinates(x, z);

            Assert.AreEqual(x, a.X);
            Assert.AreEqual(z, a.Z);
        }

        [Test]
        public void Equals_Object()
        {
            RegionCoordinates a = new RegionCoordinates(2, 3);
            GlobalColumnCoordinates b = new GlobalColumnCoordinates(2, 3);
            RegionCoordinates c = new RegionCoordinates(a.X, a.Z);

            Assert.False(a.Equals(null));
            Assert.False(a.Equals(a.ToString()));
            Assert.False(a.Equals(b));
            Assert.True(a.Equals(c));
        }

        [Test]
        public void Equals_operator()
        {
            RegionCoordinates a = new RegionCoordinates(2, 3);
            RegionCoordinates b = new RegionCoordinates(a.X, a.Z);

            Assert.False(object.ReferenceEquals(a, b));
            Assert.True(a == b);
            Assert.False(a != b);

            Assert.False(a == null);
            Assert.True(a != null);

            Assert.False(null == a);
            Assert.True(null != a);
        }

        [TestCase("<2,3>", 2, 3)]
        public void Test_ToString(string expected, int x, int z)
        {
            RegionCoordinates a = new RegionCoordinates(x, z);

            string actual = a.ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestCase(0, 0, 0, 0)]
        [TestCase(-1, 0, -WorldConstants.RegionWidth, WorldConstants.RegionDepth - 1)]
        [TestCase(0, 0, WorldConstants.RegionWidth - 1, WorldConstants.RegionDepth - 1)]
        [TestCase(1, 1, WorldConstants.RegionWidth, WorldConstants.RegionDepth)]
        [TestCase(1, 1, 2 * WorldConstants.RegionWidth - 1, 2 * WorldConstants.RegionDepth - 1)]
        [TestCase(2, 2, 2 * WorldConstants.RegionWidth, 2 * WorldConstants.RegionDepth)]
        [TestCase(-1, -1, -1, -1)]
        [TestCase(-1, -1, -WorldConstants.RegionWidth, -WorldConstants.RegionDepth)]
        [TestCase(-2, -2, -WorldConstants.RegionWidth - 1, -WorldConstants.RegionDepth - 1)]
        public void Convert_From_GlobalChunkCoordinates(int regionX, int regionZ,
                 int chunkX, int chunkZ)
        {
            GlobalChunkCoordinates chunk = new GlobalChunkCoordinates(chunkX, chunkZ);

            RegionCoordinates actual = (RegionCoordinates)chunk;

            Assert.AreEqual(regionX, actual.X);
            Assert.AreEqual(regionZ, actual.Z);
        }

        [TestCase(0, 0, 0, 0)]
        [TestCase(-1, 0, -WorldConstants.RegionWidth * WorldConstants.ChunkDepth, 0)]
        [TestCase(0, 0, WorldConstants.RegionWidth * WorldConstants.ChunkWidth - 1, WorldConstants.RegionDepth * WorldConstants.ChunkDepth -1)]
        [TestCase(1, 1, WorldConstants.RegionWidth * WorldConstants.ChunkWidth, WorldConstants.RegionDepth * WorldConstants.ChunkDepth)]
        [TestCase(1, 1, 2 * WorldConstants.RegionWidth * WorldConstants.ChunkWidth - 1, 2 * WorldConstants.RegionDepth * WorldConstants.ChunkDepth - 1)]
        [TestCase(2, 2, 2 * WorldConstants.RegionWidth * WorldConstants.ChunkWidth, 2 * WorldConstants.RegionDepth * WorldConstants.ChunkDepth)]
        [TestCase(-1, -1, -1, -1)]
        [TestCase(-1, -1, -WorldConstants.RegionWidth * WorldConstants.ChunkWidth, -WorldConstants.RegionDepth * WorldConstants.ChunkDepth)]
        [TestCase(-2, -2, -WorldConstants.RegionWidth * WorldConstants.ChunkWidth - 1, -WorldConstants.RegionDepth * WorldConstants.ChunkDepth - 1)]
        public void Convert_From_GlobalColumnCoordinates(int regionX, int regionZ,
                 int voxelX, int voxelZ)
        {
            GlobalColumnCoordinates coords = new GlobalColumnCoordinates(voxelX, voxelZ);

            RegionCoordinates actual = (RegionCoordinates)coords;

            Assert.AreEqual(regionX, actual.X);
            Assert.AreEqual(regionZ, actual.Z);
        }

        [TestCase(0, 0, 0, 0)]
        [TestCase(-1, 0, -WorldConstants.RegionWidth * WorldConstants.ChunkDepth, 0)]
        [TestCase(0, 0, WorldConstants.RegionWidth * WorldConstants.ChunkWidth - 1, WorldConstants.RegionDepth * WorldConstants.ChunkDepth - 1)]
        [TestCase(1, 1, WorldConstants.RegionWidth * WorldConstants.ChunkWidth, WorldConstants.RegionDepth * WorldConstants.ChunkDepth)]
        [TestCase(1, 1, 2 * WorldConstants.RegionWidth * WorldConstants.ChunkWidth - 1, 2 * WorldConstants.RegionDepth * WorldConstants.ChunkDepth - 1)]
        [TestCase(2, 2, 2 * WorldConstants.RegionWidth * WorldConstants.ChunkWidth, 2 * WorldConstants.RegionDepth * WorldConstants.ChunkDepth)]
        [TestCase(-1, -1, -1, -1)]
        [TestCase(-1, -1, -WorldConstants.RegionWidth * WorldConstants.ChunkWidth, -WorldConstants.RegionDepth * WorldConstants.ChunkDepth)]
        [TestCase(-2, -2, -WorldConstants.RegionWidth * WorldConstants.ChunkWidth - 1, -WorldConstants.RegionDepth * WorldConstants.ChunkDepth - 1)]
        public void Convert_From_GlobalVoxelCoordinates(int regionX, int regionZ,
                 int voxelX, int voxelZ)
        {
            GlobalVoxelCoordinates coords = new GlobalVoxelCoordinates(voxelX, _random.Next(0, 127), voxelZ);

            RegionCoordinates actual = (RegionCoordinates)coords;

            Assert.AreEqual(regionX, actual.X);
            Assert.AreEqual(regionZ, actual.Z);
        }

        [TestCase(0, 0, 0, 0, 0, 0)]
        [TestCase(0, 1, 0, 0, 0, 1)]
        [TestCase(32, 32, 1, 1, 0, 0)]
        [TestCase(63, 63, 1, 1, 31, 31)]
        [TestCase(-1, -1, -1, -1, 31, 31)]
        [TestCase(-32, -32, -1, -1, 0, 0)]
        public void GetGlobalChunkCoordinates(int expectedX, int expectedZ,
                  int regionX, int regionZ, int localChunkX, int localChunkZ)
        {
            RegionCoordinates region = new RegionCoordinates(regionX, regionZ);
            LocalChunkCoordinates local = new LocalChunkCoordinates(localChunkX, localChunkZ);

            GlobalChunkCoordinates actual = region.GetGlobalChunkCoordinates(local);

            Assert.AreEqual(expectedX, actual.X);
            Assert.AreEqual(expectedZ, actual.Z);
        }
    }
}
