using System;
using System.IO;
using System.Reflection;
using Moq;
using NUnit.Framework;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;
using TrueCraft.World;

namespace TrueCraft.Test.World
{
    [TestFixture]
    public class WorldTest
    {
        [Test]
        public void TestManifestLoaded()
        {
            Mock<IMultiplayerServer> mockWorld = new Mock<IMultiplayerServer>(MockBehavior.Strict);

            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            IWorld world = TrueCraft.World.World.LoadWorld(mockWorld.Object, Path.Combine(assemblyDir, "Files"));

            // Constants from manifest.nbt
            Assert.AreEqual(new GlobalVoxelCoordinates(0, 60, 0), world.SpawnPoint);
            Assert.AreEqual(1168393583, world.Seed);
        }
    }
}