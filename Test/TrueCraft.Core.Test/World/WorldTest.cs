using System;
using TrueCraft.Core.World;
using NUnit.Framework;
using TrueCraft.API;
using TrueCraft.Core.TerrainGen;
using TrueCraft.API.World;
using System.IO;
using System.Reflection;

namespace TrueCraft.Core.Test.World
{
    [TestFixture]
    public class WorldTest
    {
        public TrueCraft.Core.World.World World { get; set; }

        [OneTimeSetUp]
        public void SetUp()
        {
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            World = TrueCraft.Core.World.World.LoadWorld(Path.Combine(assemblyDir, "Files"));
        }

        [Test]
        public void TestMetadataLoaded()
        {
            // Constants from manifest.nbt
            Assert.AreEqual(new GlobalVoxelCoordinates(0, 60, 0), World.SpawnPoint);
            Assert.AreEqual(1168393583, World.Seed);
            Assert.IsInstanceOf<StandardGenerator>(World.ChunkProvider);
            Assert.AreEqual("default", World.Name);
        }

        [Test]
        public void TestFindChunk()
        {
            var a = World.FindChunk(new GlobalVoxelCoordinates(0, 0, 0));
            var b = World.FindChunk(new GlobalVoxelCoordinates(-1, 0, 0));
            var c = World.FindChunk(new GlobalVoxelCoordinates(-1, 0, -1));
            var d = World.FindChunk(new GlobalVoxelCoordinates(16, 0, 0));
            Assert.AreEqual(new GlobalChunkCoordinates(0, 0), a.Coordinates);
            Assert.AreEqual(new GlobalChunkCoordinates(-1, 0), b.Coordinates);
            Assert.AreEqual(new GlobalChunkCoordinates(-1, -1), c.Coordinates);
            Assert.AreEqual(new GlobalChunkCoordinates(1, 0), d.Coordinates);
        }

        [Test]
        public void TestGetChunk()
        {
            var a = World.GetChunk(new GlobalChunkCoordinates(0, 0));
            var b = World.GetChunk(new GlobalChunkCoordinates(1, 0));
            var c = World.GetChunk(new GlobalChunkCoordinates(-1, 0));
            Assert.AreEqual(new GlobalChunkCoordinates(0, 0), a.Coordinates);
            Assert.AreEqual(new GlobalChunkCoordinates(1, 0), b.Coordinates);
            Assert.AreEqual(new GlobalChunkCoordinates(-1, 0), c.Coordinates);
        }
    }
}