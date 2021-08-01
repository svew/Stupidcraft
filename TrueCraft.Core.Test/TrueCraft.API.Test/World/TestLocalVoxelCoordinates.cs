using System;
using TrueCraft.API.World;
using NUnit.Framework;

namespace TrueCraft.API.Test.World
{
    [TestFixture]
    public class TestLocalVoxelCoordinates
    {
        [TestCase(1, 2, 3)]
        public void Ctor(int x, int y, int z)
        {
            LocalVoxelCoordinates a = new LocalVoxelCoordinates(x, y, z);

            Assert.AreEqual(x, a.X);
            Assert.AreEqual(y, a.Y);
            Assert.AreEqual(z, a.Z);
        }

        [Test]
        public void Ctor_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new LocalVoxelCoordinates(-1, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LocalVoxelCoordinates(WorldConstants.ChunkWidth, 0, 0));

            Assert.Throws<ArgumentOutOfRangeException>(() => new LocalVoxelCoordinates(0, -1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LocalVoxelCoordinates(0, WorldConstants.Height, 0));

            Assert.Throws<ArgumentOutOfRangeException>(() => new LocalVoxelCoordinates(0, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LocalVoxelCoordinates(0, 0, WorldConstants.ChunkDepth));
        }

        [Test]
        public void Test_Equals_Object()
        {
            object a = new LocalVoxelCoordinates(2, 3, 5);
            object b = new LocalVoxelCoordinates(3, 3, 5);
            object c = new LocalVoxelCoordinates(2, 4, 5);
            object d = new LocalVoxelCoordinates(2, 3, 6);
            object e = new LocalVoxelCoordinates(2, 3, 5);
            object f = new GlobalVoxelCoordinates(2, 3, 5);

            Assert.False(a.Equals(b));
            Assert.False(b.Equals(a));
            Assert.False(a.Equals(c));
            Assert.False(c.Equals(a));
            Assert.False(a.Equals(d));
            Assert.False(d.Equals(a));
            Assert.True(a.Equals(e));
            Assert.True(e.Equals(a));

            Assert.False(a.Equals(null));
            Assert.False(a.Equals(f));
        }

        [Test]
        public void Test_Equals_LocalVoxelCoordinates()
        {
            LocalVoxelCoordinates a = new LocalVoxelCoordinates(2, 3, 5);
            LocalVoxelCoordinates b = new LocalVoxelCoordinates(3, 3, 5);
            LocalVoxelCoordinates c = new LocalVoxelCoordinates(2, 4, 5);
            LocalVoxelCoordinates d = new LocalVoxelCoordinates(2, 3, 6);
            LocalVoxelCoordinates e = new LocalVoxelCoordinates(2, 3, 5);

            Assert.False(a.Equals(b));
            Assert.False(b.Equals(a));
            Assert.False(a.Equals(c));
            Assert.False(c.Equals(a));
            Assert.False(a.Equals(d));
            Assert.False(d.Equals(a));
            Assert.True(a.Equals(e));
            Assert.True(e.Equals(a));

            Assert.False(a.Equals((LocalVoxelCoordinates)null));
        }

        [Test]
        public void Test_Equals_Operator()
        {
            LocalVoxelCoordinates a = new LocalVoxelCoordinates(2, 3, 5);
            LocalVoxelCoordinates b = new LocalVoxelCoordinates(a.X + 1, a.Y, a.Z);
            LocalVoxelCoordinates c = new LocalVoxelCoordinates(a.X, a.Y + 1, a.Z);
            LocalVoxelCoordinates d = new LocalVoxelCoordinates(a.X, a.Y, a.Z + 1);
            LocalVoxelCoordinates e = new LocalVoxelCoordinates(a.X, a.Y, a.Z);

            Assert.True(a == e);
            Assert.True(e == a);
            Assert.False(a != e);
            Assert.False(e != a);

            Assert.False(a == b);
            Assert.True(a != b);
            Assert.False(b == a);
            Assert.True(b != a);

            Assert.False(a == c);
            Assert.True(a != c);
            Assert.False(c == a);
            Assert.True(c != a);

            Assert.False(a == d);
            Assert.True(a != d);
            Assert.False(d == a);
            Assert.True(d != a);

            Assert.False(a == null);
            Assert.True(a != null);
            Assert.False(null == a);
            Assert.True(null != a);
        }

        [TestCase("<2,3,5>", 2, 3, 5)]
        public void Test_ToString(string expected, int x, int y, int z)
        {
            LocalVoxelCoordinates a = new LocalVoxelCoordinates(x, y, z);

            string actual = a.ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestCase(1, 2, 3, 2, 4, 6)]
        public void Add(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            LocalVoxelCoordinates a = new LocalVoxelCoordinates(x1, y1, z1);
            Vector3i b = new Vector3i(x2, y2, z2);
            LocalVoxelCoordinates expected = new LocalVoxelCoordinates(x1 + x2, y1 + y2, z1 + z2);

            LocalVoxelCoordinates actual = a.Add(b);
            Assert.AreEqual(expected, actual);

            actual = a + b;
            Assert.AreEqual(expected, actual);

            actual = b + a;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Add_Throws()
        {
            // Too far north
            LocalVoxelCoordinates a = new LocalVoxelCoordinates(5, 27, 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => a.Add(Vector3i.North));

            // Too far south
            a = new LocalVoxelCoordinates(5, 28, WorldConstants.ChunkDepth - 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => a.Add(Vector3i.South));

            // West
            a = new LocalVoxelCoordinates(0, 13, 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => a.Add(Vector3i.West));

            // East
            a = new LocalVoxelCoordinates(WorldConstants.ChunkWidth - 1, 64, 7);
            Assert.Throws<ArgumentOutOfRangeException>(() => a.Add(Vector3i.East));

            // Down
            a = new LocalVoxelCoordinates(8, 0, 8);
            Assert.Throws<ArgumentOutOfRangeException>(() => a.Add(Vector3i.Down));

            // Up
            a = new LocalVoxelCoordinates(8, WorldConstants.Height - 1, 8);
            Assert.Throws<ArgumentOutOfRangeException>(() => a.Add(Vector3i.Up));
        }

        [TestCase(0, 0, 0, 0, 0, 0)]
        [TestCase(0, 50, 0, WorldConstants.ChunkWidth, 50, 0)]
        [TestCase(0, 45, 0, 0, 45, WorldConstants.ChunkDepth)]
        [TestCase(WorldConstants.ChunkWidth - 1, 37, WorldConstants.ChunkDepth - 1,
                  3 * WorldConstants.ChunkWidth - 1, 37, 2 * WorldConstants.ChunkDepth - 1)]
        [TestCase(WorldConstants.ChunkWidth - 1, 125, WorldConstants.ChunkDepth - 1, -1, 125, -1)]
        [TestCase(0, 64, 0, -WorldConstants.ChunkWidth, 64, -WorldConstants.ChunkDepth)]
        [TestCase(WorldConstants.ChunkWidth - 1, 63, WorldConstants.ChunkDepth - 1,
                  -WorldConstants.ChunkWidth - 1, 63, -WorldConstants.ChunkDepth - 1)]
        public void Convert_From_GlobalVoxelCoordinates(int localX, int localY, int localZ,
                 int globalX, int globalY, int globalZ)
        {
            GlobalVoxelCoordinates global = new GlobalVoxelCoordinates(globalX, globalY, globalZ);

            LocalVoxelCoordinates actual = (LocalVoxelCoordinates)global;

            Assert.AreEqual(localX, actual.X);
            Assert.AreEqual(localY, actual.Y);
            Assert.AreEqual(localZ, actual.Z);
        }
    }
}
