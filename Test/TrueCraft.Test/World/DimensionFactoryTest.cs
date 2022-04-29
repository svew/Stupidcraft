using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using TrueCraft.Core.Server;
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
            Mock<IMultiplayerServer> mockServer = new Mock<IMultiplayerServer>(MockBehavior.Strict);

            IDimensionFactory factory = new DimensionFactory();

            IList<IDimensionServer> actual = factory.BuildDimensions(mockServer.Object, baseDirectory, 314159);

            Assert.AreEqual(2, actual.Count);

            Assert.IsNull(actual[0]);   // TODO Update for Nether

            IDimension overWorld = actual[1];
            Assert.AreEqual("OverWorld", overWorld.Name);
            Assert.AreEqual(DimensionID.Overworld, overWorld.ID);
        }
    }
}
