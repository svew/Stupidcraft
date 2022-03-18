using System;
using System.Collections.Generic;
using NUnit.Framework;
using TrueCraft.Core;
using TrueCraft.Core.World;
using TrueCraft.World;

namespace TrueCraft.Test.World
{
    public class DimensionFactoryTest
    {
        public DimensionFactoryTest()
        {
        }

        [Test]
        public void TestBuildDimensions()
        {
            string baseDirectory = "FakeBaseDirectory";
            IDimensionFactory factory = new DimensionFactory();

            IList<IDimension> actual = factory.BuildDimensions(baseDirectory, 314159);

            Assert.AreEqual(2, actual.Count);

            Assert.IsNull(actual[0]);   // TODO Update for Nether

            IDimension overWorld = actual[1];
            Assert.AreEqual("OverWorld", overWorld.Name);
            Assert.AreEqual(DimensionID.Overworld, overWorld.ID);
        }
    }
}
