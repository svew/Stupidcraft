using System;
using TrueCraft.Core.World;
using NUnit.Framework;


namespace TrueCraft.Core.Test.World
{
    [TestFixture]
    public class TestGlobalVoxelCoordinates
    {
        [TestCase(1, 2, 3)]
        public void ctor(int x, int y, int z)
        {
            GlobalVoxelCoordinates actual = new GlobalVoxelCoordinates(x, y, z);

            Assert.AreEqual(x, actual.X);
            Assert.AreEqual(y, actual.Y);
            Assert.AreEqual(z, actual.Z);
        }

        [TestCase(3, 5, 7)]
        public void copy_ctor(int x, int y, int z)
        {
            GlobalVoxelCoordinates expected = new GlobalVoxelCoordinates(x, y, z);

            GlobalVoxelCoordinates actual = new GlobalVoxelCoordinates(expected);

            Assert.False(object.ReferenceEquals(expected, actual));
            Assert.AreEqual(expected.X, actual.X);
            Assert.AreEqual(expected.Y, actual.Y);
            Assert.AreEqual(expected.Z, actual.Z);
        }

        [TestCase(0, 0, 0, 0, 0, 0, 0, 0)]
        [TestCase(WorldConstants.ChunkWidth, 15, 0,
            1, 0, 0, 15, 0)]
        [TestCase(0, 17, WorldConstants.ChunkDepth,
            0, 1, 0, 17, 0)]
        [TestCase(2 * WorldConstants.ChunkWidth, 17, 2 * WorldConstants.ChunkWidth,
            2, 2, 0, 17, 0)]
        [TestCase(2 * WorldConstants.ChunkWidth - 1, 19, 2 * WorldConstants.ChunkDepth - 1,
            1, 1, WorldConstants.ChunkWidth - 1, 19, WorldConstants.ChunkDepth - 1)]
        [TestCase(-1, 21, -1,
            -1, -1, WorldConstants.ChunkWidth - 1, 21, WorldConstants. ChunkDepth - 1)]
        [TestCase(-WorldConstants.ChunkWidth, 23, -WorldConstants.ChunkDepth,
            -1, -1, 0, 23, 0)]
        public void From_Chunk_And_Local(int expectedX, int expectedY, int expectedZ,
                  int chunkX, int chunkZ, int localX, int localY, int localZ)
        {
            GlobalChunkCoordinates chunk = new GlobalChunkCoordinates(chunkX, chunkZ);
            LocalVoxelCoordinates local = new LocalVoxelCoordinates(localX, localY, localZ);

            GlobalVoxelCoordinates actual = GlobalVoxelCoordinates.GetGlobalVoxelCoordinates(chunk, local);

            Assert.AreEqual(expectedX, actual.X);
            Assert.AreEqual(expectedY, actual.Y);
            Assert.AreEqual(expectedZ, actual.Z);
        }

        [TestCase(true, 1, 2, 3, 1, 2, 3)]
        [TestCase(false, 1, 3, 5, 2, 3, 5)]  // x not equal 
        [TestCase(false, 2, 2, 5, 2, 3, 5)]  // y not equal
        [TestCase(false, 2, 3, 5, 2, 3, 7)]  // z not equal
        public void Equals(bool expected, int x1, int y1, int z1, int x2, int y2, int z2)
        {
            GlobalVoxelCoordinates a = new GlobalVoxelCoordinates(x1, y1, z1);
            GlobalVoxelCoordinates b = new GlobalVoxelCoordinates(x2, y2, z2);

            bool actual = (a.Equals(b));

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Equals_other()
        {
            GlobalVoxelCoordinates a = new GlobalVoxelCoordinates(2, 3, 5);

            // Not null is not equal to null
            Assert.False(a.Equals(null));
            Assert.False(a!.Equals((GlobalVoxelCoordinates?)null));

            // Cannot be equal to a different type
            Assert.False(a!.Equals(new LocalVoxelCoordinates(a.X, a.Y, a.Z)));
        }

        [TestCase(true, 1, 2, 3, 1, 2, 3)]
        [TestCase(false, 1, 3, 5, 2, 3, 5)]  // x not equal 
        [TestCase(false, 2, 2, 5, 2, 3, 5)]  // y not equal
        [TestCase(false, 2, 3, 5, 2, 3, 7)]  // z not equal
        public void Equals_op(bool expected, int x1, int y1, int z1, int x2, int y2, int z2)
        {
            GlobalVoxelCoordinates a = new GlobalVoxelCoordinates(x1, y1, z1);
            GlobalVoxelCoordinates b = new GlobalVoxelCoordinates(x2, y2, z2);

            // == operator
            bool actual = (a == b);

            Assert.AreEqual(expected, actual);

            // != operator
            actual = (a != b);

            Assert.AreNotEqual(expected, actual);
        }

        [TestCase("<2,3,5>", 2, 3, 5)]
        public void GlobalVoxelCoordinatess_ToString(string expected, int x, int y, int z)
        {
            GlobalVoxelCoordinates a = new GlobalVoxelCoordinates(x, y, z);

            string actual = a.ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestCase(2, 3, 5, 7, 11, 13)]
        public void DistanceTo_GlobalVoxelCoordinates(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            GlobalVoxelCoordinates a = new GlobalVoxelCoordinates(x1, y1, z1);
            GlobalVoxelCoordinates b = new GlobalVoxelCoordinates(x2, y2, z2);
            int dx = x2 - x1;
            int dy = y2 - y1;
            int dz = z2 - z1;
            double expected = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            double actual = a.DistanceTo(b);
            Assert.AreEqual(expected, actual);

            actual = b.DistanceTo(a);
            Assert.AreEqual(expected, actual);
        }

        [TestCase(2, 3, 5, 7, 11, 13)]
        public void DistanceTo_Vector3(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            GlobalVoxelCoordinates a = new GlobalVoxelCoordinates(x1, y1, z1);
            Vector3 b = new Vector3(x2, y2, z2);
            double dx = x2 - (x1 + 0.5);
            double dy = y2 - (y1 + 0.5);
            double dz = z2 - (z1 + 0.5);
            double expected = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            double actual = a.DistanceTo(b);
            Assert.AreEqual(expected, actual);
        }

        [TestCase(1, 2, 3, 2, 4, 6)]
        public void Add(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            GlobalVoxelCoordinates a = new GlobalVoxelCoordinates(x1, y1, z1);
            Vector3i b = new Vector3i(x2, y2, z2);
            GlobalVoxelCoordinates expected = new GlobalVoxelCoordinates(x1 + x2, y1 + y2, z1 + z2);

            GlobalVoxelCoordinates actual = a.Add(b);
            Assert.AreEqual(expected, actual);

            actual = a + b;
            Assert.AreEqual(expected, actual);

            actual = b + a;
            Assert.AreEqual(expected, actual);
        }

        [TestCase(4, 5, 6, 1, 0, -1)]
        public void Subtract(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            GlobalVoxelCoordinates a = new GlobalVoxelCoordinates(x1, y1, z1);
            GlobalVoxelCoordinates b = new GlobalVoxelCoordinates(x2, y2, z2);
            int dx = x1 - x2;
            int dy = y1 - y2;
            int dz = z1 - z2;

            Vector3i expected = new Vector3i(dx, dy, dz);

            Vector3i actual = a - b;

            Assert.AreEqual(expected, actual);

            expected = new Vector3i(-dx, -dy, -dz);
            actual = b - a;

            Assert.AreEqual(expected, actual);
        }

        [TestCase(1, 2, 3)]
        public void Negate(int x, int y, int z)
        {
            GlobalVoxelCoordinates a = new GlobalVoxelCoordinates(x, y, z);
            GlobalVoxelCoordinates expected = new GlobalVoxelCoordinates(-x, -y, -z);

            GlobalVoxelCoordinates actual = -a;

            Assert.AreEqual(expected, actual);
        }

        [TestCase(3, 1)]
        public void Convert_GlobalColumnCoordinates(int x, int z)
        {
            GlobalColumnCoordinates a = new GlobalColumnCoordinates(x, z);

            GlobalVoxelCoordinates actual = (GlobalVoxelCoordinates)a;

            Assert.AreEqual(x, actual.X);
            Assert.AreEqual(0, actual.Y);
            Assert.AreEqual(z, actual.Z);
        }

        [TestCase(3.14, 1.41, 2.72)]
        public void Convert_Vector3(double x, double y, double z)
        {
            Vector3 a = new Vector3(x, y, z);
            GlobalVoxelCoordinates expected = new GlobalVoxelCoordinates((int)x, (int)y, (int)z);

            GlobalVoxelCoordinates actual = (GlobalVoxelCoordinates)a;

            Assert.AreEqual(expected, actual);
        }

        [TestCase(0, 0, 0, 0)]
        [TestCase(2 * WorldConstants.ChunkWidth, WorldConstants.ChunkDepth, 2, 1)]
        [TestCase(-WorldConstants.ChunkWidth, -2 * WorldConstants.ChunkDepth, -1, -2)]
        public void Convert_GlobalChunkCoordinates(int expectedX, int expectedZ, int chunkX, int chunkZ)
        {
            GlobalChunkCoordinates a = new GlobalChunkCoordinates(chunkX, chunkZ);
            GlobalVoxelCoordinates expected = new GlobalVoxelCoordinates(expectedX, 0, expectedZ);

            GlobalVoxelCoordinates actual = (GlobalVoxelCoordinates)a;

            Assert.AreEqual(expected, actual);
        }
    }
}
