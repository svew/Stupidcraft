using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using TrueCraft.Core.World;
using TrueCraft.World;

namespace TrueCraft.Test.World
{
    [TestFixture]
    public class WorldTest
    {
        private IWorld _world;

        [OneTimeSetUp]
        public void SetUp()
        {
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _world = TrueCraft.World.World.LoadWorld(Path.Combine(assemblyDir, "Files"));
        }

        [Test]
        public void TestManifestLoaded()
        {
            // Constants from manifest.nbt
            Assert.AreEqual(new GlobalVoxelCoordinates(0, 60, 0), _world.SpawnPoint);
            Assert.AreEqual(1168393583, _world.Seed);
        }
    }
}