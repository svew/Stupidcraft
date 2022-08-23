using System;
using System.Collections.Generic;
using NUnit.Framework;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Test.World
{
    public class TestRay
    {
        [Test]
        public void Ctor()
        {
            Vector3 pos = new Vector3(1, 2, 3);
            Vector3 direction = new Vector3(0.2, 0.3, 0.5);

            Ray actual = new Ray(pos, direction);

            Assert.AreEqual(pos.X, actual.Position.X);
            Assert.AreEqual(pos.Y, actual.Position.Y);
            Assert.AreEqual(pos.Z, actual.Position.Z);

            Assert.AreEqual(direction.X, actual.Direction.X);
            Assert.AreEqual(direction.Y, actual.Direction.Y);
            Assert.AreEqual(direction.Z, actual.Direction.Z);
        }

        public static IEnumerable<object[]> EqualityTestData()
        {
            // Test case where the Ray objects are equal
            yield return new object[] { true,
                new Ray(new Vector3(1, 2, 3), new Vector3(0.1, 0.2, 0.3)),
                new Ray(new Vector3(1, 2, 3), new Vector3(0.1, 0.2, 0.3))
            };

            // Test that inequality results when any single component of the
            // Ray is altered.
            yield return new object[] { false,
                new Ray(new Vector3(1, 2, 3), new Vector3(0.1, 0.2, 0.3)),
                new Ray(new Vector3(5, 2, 3), new Vector3(0.1, 0.2, 0.3))
            };

            yield return new object[] { false,
                new Ray(new Vector3(1, 2, 3), new Vector3(0.1, 0.2, 0.3)),
                new Ray(new Vector3(1, 5, 3), new Vector3(0.1, 0.2, 0.3))
            };

            yield return new object[] { false,
                new Ray(new Vector3(1, 2, 3), new Vector3(0.1, 0.2, 0.3)),
                new Ray(new Vector3(1, 2, 5), new Vector3(0.1, 0.2, 0.3))
            };

            yield return new object[] { false,
                new Ray(new Vector3(1, 2, 3), new Vector3(0.1, 0.2, 0.3)),
                new Ray(new Vector3(1, 2, 3), new Vector3(0.5, 0.2, 0.3))
            };

            yield return new object[] { false,
                new Ray(new Vector3(1, 2, 3), new Vector3(0.1, 0.2, 0.3)),
                new Ray(new Vector3(1, 2, 3), new Vector3(0.1, 0.5, 0.3))
            };

            yield return new object[] { false,
                new Ray(new Vector3(1, 2, 3), new Vector3(0.1, 0.2, 0.3)),
                new Ray(new Vector3(1, 2, 3), new Vector3(0.1, 0.2, 0.5))
            };
        }

        [TestCaseSource(nameof(EqualityTestData))]
        public void Equals_obj(bool expected, Ray ray1, Ray ray2)
        {
            Assert.AreEqual(expected, ray1.Equals((object)ray2));
            Assert.AreEqual(expected, ray2.Equals((object)ray1));
        }

        [TestCaseSource(nameof(EqualityTestData))]
        public void Equals_Ray(bool expected, Ray ray1, Ray ray2)
        {
            Assert.AreEqual(expected, ray1.Equals(ray2));
            Assert.AreEqual(expected, ray2.Equals(ray1));

            Assert.AreEqual(expected, ray1 == ray2);
            Assert.AreEqual(expected, ray2 == ray1);
            Assert.AreEqual(!expected, ray1 != ray2);
            Assert.AreEqual(!expected, ray2 != ray1);
        }

        public static IEnumerable<object[]> IntersectsTestData()
        {
            // A ray that crosses no planes of the box, and therefore
            // cannot intersect it.
            yield return new object[]
            {
                0,
                false, 0, BlockFace.PositiveY,
                new Ray(new Vector3(-5, -5, -5), new Vector3(10, 10, 10)),
                new BoundingBox(new Vector3(300, 30, 250), new Vector3(304, 34, 254))
            };

            // A Ray that is aimed at the negative-X surface of the box,
            // but stops short of it, and therefore does not intersect it.
            // t in the range 0..1
            //  x = 4 * t - 3
            //  y = -1 * t + 13.2
            //  z = 2 * t - 0.8
            // It would intersects the box at t = 2; (x,y,z) = (0.3, 0.05, 1)
            yield return new object[]
            {
                1,
                false, double.MinValue, BlockFace.NegativeX,
                new Ray(new Vector3(-3, 13.2, -0.8), new Vector3(4, -1, 2)),
                new BoundingBox(new Vector3(5, 11, 3), new Vector3(6, 12, 4))
            };

            // A Ray that starts outside the positive-X surface and points
            // away from the Box.  It does NOT intersect the Box.
            // t in the range 0..1
            //  x = 5t + 4.5
            //  y = -4t - 2.5
            //  z = -3t - 2.05
            // It would intersects the box at t = -0.7; (x,y,z) = (1, 0.3, 0.05)
            yield return new object[]
            {
                2,
                false, double.MinValue, BlockFace.PositiveX,
                new Ray(new Vector3(4.5, -2.5, -2.05), new Vector3(5, -4, -3)),
                new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1))
            };

            // A ray that enters the negative-X surface of the box at an angle.
            // The Ray is defined by the following equations:
            // t in the range 0..1
            //  x = 10 * t - 5
            //  y = -10 * t + 6
            //  z = 10 * t - 4
            // It intersects the box at t = 0.5, (x,y,z) = (0, 1, 1)
            yield return new object[]
            {
                3,
                true, 0.5, BlockFace.NegativeX,
                new Ray(new Vector3(-5, 6, -4), new Vector3(10, -9, 11)),
                new BoundingBox(new Vector3(0, 0, 0), new Vector3(3, 3, 3))
            };

            // This ray enters the positive-X surface of the Box, and
            // ends within the box.
            // t in the range 0..1
            //  x = -5t + 4.5
            //  y = 4t - 2.5
            //  z = 3t - 2.05
            // It intersects the box at t = 0.7; (x,y,z) = (1, 0.3, 0.05)
            yield return new object[]
            {
                4,
                true, 0.7, BlockFace.PositiveX,
                new Ray(new Vector3(4.5, -2.5, -2.05), new Vector3(-5, 4, 3)),
                new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1))
            };

            // This ray enters the negative-Y surface of the box.
            // t in the range 0..1
            //  x = -9 * t + 11
            //  y = 10 * t - 5
            //  z = 11 * t - 4
            // It intersects the box at t = 0.5, (x,y,z) = (6.5, 0, 1.5)
            yield return new object[]
            {
                5,
                true, 0.5, BlockFace.NegativeY,
                new Ray(new Vector3(11, -5, -4), new Vector3(-9, 10, 11)),
                new BoundingBox(new Vector3(5, 0, 0), new Vector3(8, 3, 3))
            };

            // This ray enters the positive-Y surface of the Box, and
            // ends within the box.
            // t in the range 0..1
            //  x = 4t - 2.5
            //  y = -5t + 4.5
            //  z = 3t - 2.05
            // It intersects the box at t = 0.7; (x,y,z) = (0.3, 1, 0.05)
            yield return new object[]
            {
                6,
                true, 0.7, BlockFace.PositiveY,
                new Ray(new Vector3(-2.5, 4.5, -2.05), new Vector3(4, -5, 3)),
                new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1))
            };

            // This ray enters the negative-Z surface of the box.
            // t in the range 0..1
            //  x = -9 * t + 11
            //  y = 11 * t - 4
            //  z = 10 * t - 5
            // It intersects the box at t = 0.5, (x,y,z) = (6.5, 1.5, 0)
            yield return new object[]
            {
                7,
                true, 0.5, BlockFace.NegativeZ,
                new Ray(new Vector3(11, -4, -5), new Vector3(-9, 11, 10)),
                new BoundingBox(new Vector3(5, 0, 0), new Vector3(8, 3, 3))
            };

            // This ray enters the positive-Z surface of the Box, and
            // ends within the box.
            // t in the range 0..1
            //  x = 4t - 2.5
            //  y = 3t - 2.05
            //  z = -5t + 4.5
            // It intersects the box at t = 0.7; (x,y,z) = (0.3, 0.05, 1)
            yield return new object[]
            {
                8,
                true, 0.7, BlockFace.PositiveZ,
                new Ray(new Vector3(-2.5, -2.05, 4.5), new Vector3(4, 3, -5)),
                new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1))
            };

            // This Ray starts on the Positive-X surface and points away.
            //
            yield return new object[]
            {
                9,
                true, 0.0, BlockFace.PositiveX,
                new Ray(new Vector3(5, 7.5, 11.5), new Vector3(7, 6, 7)),
                new BoundingBox(new Vector3(4, 7, 11), new Vector3(5, 8, 12))
            };

            // This Ray starts on the Negative-X surface and points away.
            yield return new object[]
            {
                10,
                true, 0.0, BlockFace.NegativeX,
                new Ray(new Vector3(5, 11.2, 3.2), new Vector3(-4, 1, -2)),
                new BoundingBox(new Vector3(5, 11, 3), new Vector3(6, 12, 4))
            };

            // This Ray starts on the Positive-Y surface and points away.
            yield return new object[]
            {
                11,
                true, 0.0, BlockFace.PositiveY,
                new Ray(new Vector3(-42.1, 63, -200.75), new Vector3(3, 4, -7)),
                new BoundingBox(new Vector3(-43, 62, -201), new Vector3(-42, 63, -200))
            };

            // This Ray starts on the Negative-Y surface and points away.
            yield return new object[]
            {
                12,
                true, 0.0, BlockFace.NegativeY,
                new Ray(new Vector3(-256.3, 100, 1032.7), new Vector3(5, -3, 0)),
                new BoundingBox(new Vector3(-257, 100, 1032), new Vector3(-256, 101, 1033))
            };

            // This Ray starts on the Negative-Z surface and points away.
            yield return new object[]
            {
                13,
                true, 0.0, BlockFace.NegativeZ,
                new Ray(new Vector3(3217.6, 64.5, 497), new Vector3(0, 0, -5)),
                new BoundingBox(new Vector3(3217, 64, 497), new Vector3(3218, 65, 497))
            };

            // This Ray starts on the Positive-Z surface and points away.
            yield return new object[]
            {
                14,
                true, 0.0, BlockFace.PositiveZ,
                new Ray(new Vector3(217.4, 63.75, -354), new Vector3(0.02, -.3, 3)),
                new BoundingBox(new Vector3(217, 63, -354), new Vector3(281, 64, -354))
            };

            // The Ray is entirely inside the Box.  Therefore, it intersects the box.
            // However, strictly speaking, it does not intersect the surface of the box.
            yield return new object[]
            {
                15,
                true, 0.0, BlockFace.PositiveY,
                new Ray(new Vector3(-6, 63.782, 194), new Vector3(0, -0.787, 0)),
                new BoundingBox(new Vector3(-7.3, 61.19, 192.7), new Vector3(-5.7, 63.81, 194.3))
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serial">A unique number used to identify the test by its
        /// parameters when viewed in the GUI.  This is ignored within the test.</param>
        /// <param name="expectedToIntersect"></param>
        /// <param name="expectedDistance"></param>
        /// <param name="expectedBlockFace"></param>
        /// <param name="ray"></param>
        /// <param name="box"></param>
        [TestCaseSource(nameof(IntersectsTestData))]
        public void Intersects(int serial,
            bool expectedToIntersect, double expectedDistance, BlockFace expectedBlockFace,
            Ray ray, BoundingBox box)
        {
            BlockFace actualBlockFace = BlockFace.PositiveY;
            double actualDistance = double.MinValue;

            bool actualIntersects = ray.Intersects(box, ref actualDistance, ref actualBlockFace);

            if (expectedToIntersect)
            {
                Assert.True(actualIntersects);
                Assert.True(Math.Abs(expectedDistance - actualDistance) < GameConstants.Epsilon,
                    "Expected Distance: {0}\nActual Distance: {1}", expectedDistance, actualDistance);
                Assert.AreEqual(expectedBlockFace, actualBlockFace);
            }
            else
            {
                Assert.False(actualIntersects);
            }
        }
    }
}

