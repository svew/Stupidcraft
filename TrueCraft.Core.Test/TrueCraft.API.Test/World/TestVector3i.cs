using System;
using TrueCraft.API.World;
using NUnit.Framework;

namespace TrueCraft.API.Test.World
{
    [TestFixture]
    public class TestVector3i
    {
        [TestCase(1, 2, 3)]
        public void Vector3i_Constructor(int x, int y, int z)
        {
            Vector3i actual = new Vector3i(x, y, z);

            Assert.AreEqual(x, actual.X);
            Assert.AreEqual(y, actual.Y);
            Assert.AreEqual(z, actual.Z);
        }

        [Test]
        public void Vector3i_Equality()
        {
            Vector3i a, b;

            a = new Vector3i(1, 2, 3);
            b = new Vector3i(3, 2, 1);

            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);

            b = new Vector3i(1, 2, 3);
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }

        [TestCase(2, 4, 6, 1, 0, 0, 3, 4, 5)]
        [TestCase(2, 4, 6, 0, 1, 0, 2, 5, 6)]
        [TestCase(2, 4, 6, 0, 0, 1, 2, 4, 7)]
        public void Vector3i_Add(int x1, int y1, int z1, int x2, int y2, int z2,
                  int expectedX, int expectedY, int expectedZ)
        {
            Vector3i a = new Vector3i(x1, y1, z1);
            Vector3i b = new Vector3i(x2, y2, z2);
            Vector3i expected = new Vector3i(expectedX, expectedY, expectedZ);

            Vector3i actual = a + b;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Vector3i_ToString()
        {
            Vector3i a = new Vector3i(2, 3, 5);

            string actual = a.ToString();

            Assert.AreEqual("<2,3,5>", actual);
        }
    }
}
