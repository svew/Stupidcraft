using System;
using TrueCraft.Core.World;
using NUnit.Framework;

namespace TrueCraft.Core.Test.World
{
    [TestFixture]
    public class TestLocalChunkCoordinates
    {
        private Random _random;

        public TestLocalChunkCoordinates()
        {
            _random = new Random(1234);
        }

        [TestCase(2, 3)]
        public void Test_ctor(int x, int z)
        {
            LocalChunkCoordinates actual = new LocalChunkCoordinates(x, z);

            Assert.AreEqual(x, actual.X);
            Assert.AreEqual(z, actual.Z);
        }

#if DEBUG
        [Test]
        public void Test_ctor_throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new LocalChunkCoordinates(-1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LocalChunkCoordinates(0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LocalChunkCoordinates(WorldConstants.RegionWidth, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LocalChunkCoordinates(0, WorldConstants.RegionDepth));
        }
#endif

        [Test]
        public void Test_Equals_Object()
        {
            LocalChunkCoordinates a = new LocalChunkCoordinates(2, 5);
            LocalChunkCoordinates b = new LocalChunkCoordinates(3, 7);
            LocalChunkCoordinates c = new LocalChunkCoordinates(a.X, a.Z);
            GlobalChunkCoordinates d = new GlobalChunkCoordinates(a.X, a.Z);

            Assert.True(a.Equals((object)c));
            Assert.True(c.Equals((object)a));

            Assert.False(a.Equals((object)b));
            Assert.False(b.Equals((object)a));

            Assert.False(a.Equals(null));
            Assert.False(a!.Equals("some string"));

            Assert.False(a.Equals(d));
        }

        [Test]
        public void Test_Equals_LocalChunkCoordinates()
        {
            LocalChunkCoordinates a = new LocalChunkCoordinates(7, 11);
            LocalChunkCoordinates b = new LocalChunkCoordinates(5, 3);

            Assert.False(a.Equals(b));
            Assert.False(b.Equals(a));

            Assert.False(a.Equals((LocalChunkCoordinates?)null));
        }

        [Test]
        public void Test_Equality_operator()
        {
            LocalChunkCoordinates a = new LocalChunkCoordinates(2, 3);
            LocalChunkCoordinates b = new LocalChunkCoordinates(3, 2);
            LocalChunkCoordinates c = new LocalChunkCoordinates(2, 3);

            Assert.False(a == b);
            Assert.True(a != b);

            Assert.False(b == a);
            Assert.True(b != a);

            Assert.True(a == c);
            Assert.False(a != c);

            Assert.True(c == a);
            Assert.False(c != a);

            Assert.False(a == null);
            Assert.True(a != null);

            Assert.False(null == a);
            Assert.True(null != a);
        }

        [TestCase("<2,3>", 2, 3)]
        public void Test_ToString(string expected, int x, int z)
        {
            LocalChunkCoordinates a = new LocalChunkCoordinates(x, z);

            string actual = a.ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestCase(0, 0, 0, 0)]
        [TestCase(WorldConstants.RegionWidth - 1, WorldConstants.RegionDepth - 1, WorldConstants.RegionWidth - 1, WorldConstants.RegionDepth - 1)]
        [TestCase(2, 0, 2, WorldConstants.RegionDepth)]
        [TestCase(0, 1, WorldConstants.RegionWidth, 1)]
        [TestCase(WorldConstants.RegionWidth - 1, WorldConstants.RegionDepth - 1, 2 * WorldConstants.RegionWidth - 1, 2 * WorldConstants.RegionDepth - 1)]
        [TestCase(WorldConstants.RegionWidth - 1, WorldConstants.RegionDepth - 1, -1, -1)]
        [TestCase(0, 0, -WorldConstants.RegionWidth, -WorldConstants.RegionDepth)]
        [TestCase(WorldConstants.RegionWidth - 1, WorldConstants.RegionDepth - 1,
                  -WorldConstants.RegionWidth - 1, -WorldConstants.RegionDepth - 1)]
        public void Convert_From_GlobalChunkCoordinates(int localX, int localZ, int globalX, int globalZ)
        {
            GlobalChunkCoordinates global = new GlobalChunkCoordinates(globalX, globalZ);

            LocalChunkCoordinates actual = (LocalChunkCoordinates)global;

            Assert.AreEqual(localX, actual.X);
            Assert.AreEqual(localZ, actual.Z);
        }

        [TestCase(0, 0, 0, 0)]
        [TestCase(1, 0, WorldConstants.ChunkWidth, 0)]
        [TestCase(0, 1, 0, WorldConstants.ChunkDepth)]
        [TestCase(WorldConstants.RegionWidth - 1, WorldConstants.RegionDepth - 1,
                  WorldConstants.RegionWidth * WorldConstants.ChunkWidth - 1,
                  WorldConstants.RegionDepth * WorldConstants.ChunkDepth - 1)]
        [TestCase(0, 0,
                  WorldConstants.RegionWidth * WorldConstants.ChunkWidth,
                  WorldConstants.RegionDepth * WorldConstants.ChunkDepth)]
        [TestCase(WorldConstants.RegionWidth - 1, WorldConstants.RegionDepth - 1, -1, -1)]
        [TestCase(0, 0, -WorldConstants.RegionWidth * WorldConstants.ChunkWidth,
                  -WorldConstants.RegionDepth * WorldConstants.ChunkDepth)]
        [TestCase(WorldConstants.RegionWidth - 1, WorldConstants.RegionDepth - 1,
                  -WorldConstants.RegionWidth * WorldConstants.ChunkWidth - 1,
                  -WorldConstants.RegionDepth * WorldConstants.ChunkDepth - 1)]
        public void Convert_From_GlobalVoxelCoordinates(int localX, int localZ, int globalX, int globalZ)
        {
            GlobalVoxelCoordinates global = new GlobalVoxelCoordinates(globalX, _random.Next(127), globalZ);

            LocalChunkCoordinates actual = (LocalChunkCoordinates)global;

            Assert.AreEqual(localX, actual.X);
            Assert.AreEqual(localZ, actual.Z);
        }
    }
}
