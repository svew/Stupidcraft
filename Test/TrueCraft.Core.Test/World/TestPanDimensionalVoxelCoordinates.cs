using System;
using TrueCraft.Core.World;
using NUnit.Framework;
using Moq;

namespace TrueCraft.Core.Test.World
{
    public class TestPanDimensionalVoxelCoordinates
    {
        public TestPanDimensionalVoxelCoordinates()
        {
        }

        [TestCase(DimensionID.Overworld, 10, 20, 30)]
        [TestCase(DimensionID.Nether, 499, 127, 65537)]
        public void ctor(DimensionID dimensionID, int x, int y, int z)
        {
            PanDimensionalVoxelCoordinates actual = new PanDimensionalVoxelCoordinates(dimensionID, x, y, z);

            Assert.AreEqual(actual.DimensionID, dimensionID);
            Assert.AreEqual(actual.X, x);
            Assert.AreEqual(actual.Y, y);
            Assert.AreEqual(actual.Z, z);
        }

        [TestCase(true, DimensionID.Overworld, 2, 3, 5, DimensionID.Overworld, 2, 3, 5)]
        [TestCase(false, DimensionID.Overworld, 3, 5, 7, DimensionID.Nether, 3, 5, 7)]
        [TestCase(false, DimensionID.Overworld, 5, 7, 11, DimensionID.Overworld, 3, 7, 11)]
        [TestCase(false, DimensionID.Nether, 7, 11, 13, DimensionID.Nether, 7, 5, 13)]
        [TestCase(false, DimensionID.Overworld, 11, 13, 17, DimensionID.Overworld, 11, 13, 7)]
        public void Equals(bool expected, DimensionID id1, int x1, int y1, int z1, DimensionID id2, int x2, int y2, int z2)
        {
            PanDimensionalVoxelCoordinates a = new PanDimensionalVoxelCoordinates(id1, x1, y1, z1);
            PanDimensionalVoxelCoordinates b = new PanDimensionalVoxelCoordinates(id2, x2, y2, z2);

            bool actual = a.Equals(b);
            Assert.AreEqual(expected, actual);

            actual = (a == b);
            Assert.AreEqual(expected, actual);

            actual = (a != b);
            Assert.AreEqual(expected, !actual);
        }

        [Test]
        public void Equals_other()
        {
            PanDimensionalVoxelCoordinates a = new PanDimensionalVoxelCoordinates(DimensionID.Overworld, 2, 3, 5);

            // Not null is not equal to null
            Assert.False(a.Equals(null));
            Assert.False(a!.Equals((PanDimensionalVoxelCoordinates?)null));

            // Cannot be equal to a different type
            Assert.False(a!.Equals(new LocalVoxelCoordinates(a.X, a.Y, a.Z)));
        }

    }
}
