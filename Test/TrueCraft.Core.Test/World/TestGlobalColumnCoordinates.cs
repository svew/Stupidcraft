using System;
using TrueCraft.Core.World;
using NUnit.Framework;

namespace TrueCraft.Core.Test.World
{
    [TestFixture]
    public class TestGlobalColumnCoordinates
    {
        [TestCase(2, 3)]
        public void Test_ctor(int x, int z)
        {
            GlobalColumnCoordinates a = new GlobalColumnCoordinates(x, z);

            Assert.AreEqual(x, a.X);
            Assert.AreEqual(z, a.Z);
        }

        [Test]
        public void Test_Equals_Object()
        {
            GlobalColumnCoordinates a = new GlobalColumnCoordinates(3, 5);
            GlobalVoxelCoordinates b = new GlobalVoxelCoordinates(a.X, 42, a.Z);
            GlobalColumnCoordinates c = new GlobalColumnCoordinates(a.X, a.Z);

            Assert.False(a.Equals(null));
            Assert.False(a.Equals(b));
            Assert.False(a.Equals(a.ToString()));
            Assert.True(a.Equals((object)c));
        }

        [Test]
        public void Test_Equals_GlobalColumnCoordinates()
        {
            GlobalColumnCoordinates a = new GlobalColumnCoordinates(3, 5);
            GlobalColumnCoordinates b = new GlobalColumnCoordinates(5, 3);
            GlobalColumnCoordinates c = new GlobalColumnCoordinates(a.X, a.Z);

            Assert.True(a.Equals(c));
            Assert.True(c.Equals(a));
            Assert.False(a.Equals(b));
            Assert.False(a.Equals((GlobalColumnCoordinates)null));
        }

        [Test]
        public void Test_Equality_op()
        {
            GlobalColumnCoordinates a = new GlobalColumnCoordinates(3, 5);
            GlobalColumnCoordinates b = new GlobalColumnCoordinates(5, 3);
            GlobalColumnCoordinates c = new GlobalColumnCoordinates(a.X, a.Z);

            Assert.True(a == c);
            Assert.False(a != c);

            Assert.False(a == b);
            Assert.True(a != b);

            Assert.False(a == null);
            Assert.True(a != null);

            Assert.False(null == b);
            Assert.True(null != b);
        }

        [TestCase("<2,3>", 2, 3)]
        public void Test_ToString(string expected, int x, int z)
        {
            GlobalColumnCoordinates a = new GlobalColumnCoordinates(x, z);

            string actual = a.ToString();

            Assert.AreEqual(expected, actual);
        }
    }
}
