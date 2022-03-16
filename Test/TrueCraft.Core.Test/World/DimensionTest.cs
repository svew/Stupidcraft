using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using TrueCraft.Core.TerrainGen;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Test.World
{
    [TestFixture]
    public class DimensionTest
    {
        public IDimension Dimension { get; set; }

        [OneTimeSetUp]
        public void SetUp()
        {
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Dimension = TrueCraft.Core.World.Dimension.LoadWorld(Path.Combine(assemblyDir, "Files"));
        }

        [Test]
        public void TestMetadataLoaded()
        {
            // Constants from manifest.nbt
            Assert.AreEqual(new GlobalVoxelCoordinates(0, 60, 0), Dimension.SpawnPoint);
            Assert.AreEqual(1168393583, Dimension.Seed);
            Assert.IsInstanceOf<StandardGenerator>(Dimension.ChunkProvider);
            Assert.AreEqual("default", Dimension.Name);
        }

        [Test]
        public void TestFindChunk()
        {
            var a = Dimension.FindChunk(new GlobalVoxelCoordinates(0, 0, 0));
            var b = Dimension.FindChunk(new GlobalVoxelCoordinates(-1, 0, 0));
            var c = Dimension.FindChunk(new GlobalVoxelCoordinates(-1, 0, -1));
            var d = Dimension.FindChunk(new GlobalVoxelCoordinates(16, 0, 0));
            Assert.AreEqual(new GlobalChunkCoordinates(0, 0), a.Coordinates);
            Assert.AreEqual(new GlobalChunkCoordinates(-1, 0), b.Coordinates);
            Assert.AreEqual(new GlobalChunkCoordinates(-1, -1), c.Coordinates);
            Assert.AreEqual(new GlobalChunkCoordinates(1, 0), d.Coordinates);
        }

        [Test]
        public void TestGetChunk()
        {
            var a = Dimension.GetChunk(new GlobalChunkCoordinates(0, 0));
            var b = Dimension.GetChunk(new GlobalChunkCoordinates(1, 0));
            var c = Dimension.GetChunk(new GlobalChunkCoordinates(-1, 0));
            Assert.AreEqual(new GlobalChunkCoordinates(0, 0), a.Coordinates);
            Assert.AreEqual(new GlobalChunkCoordinates(1, 0), b.Coordinates);
            Assert.AreEqual(new GlobalChunkCoordinates(-1, 0), c.Coordinates);
        }
    }
}