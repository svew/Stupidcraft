using System;
using TrueCraft.API.World;
using TrueCraft.Core.World;
using NUnit.Framework;


namespace TrueCraft.Core.Test.World
{
    [TestFixture]
    public class CoordinatesTest
    {
        [TestCase(0, 0, 0, 0)]      // origin maps to origin
        [TestCase(Chunk.Width - 1, 0, Chunk.Width - 1, 0)]    // going east - last block in chunk 0, 0 maps to 15,0
        [TestCase(Chunk.Width, 0, 0, 0)]                      // going east - first block in chunk 1,0 maps to 0, 0
        [TestCase(0, Chunk.Depth - 1, 0, Chunk.Depth - 1)]    // going south - last block in chunk 0,0 maps to 0,15
        [TestCase(0, Chunk.Depth, 0, 0)]                      // going south - first block in chunk 0,1 maps to 0,0
        [TestCase(-1, 0, Chunk.Width - 1, 0)]                 // going west - first block in chunk -1,0 maps to 15,0
        [TestCase(-Chunk.Width, 0, 0, 0)]                     // going west - last block in chunk -1,0 maps to 0, 0
        [TestCase(-Chunk.Width - 1, 0, Chunk.Width - 1, 0)]   // going west - first block in chunk -2,0 maps to 15,0
        [TestCase(0, -1, 0, Chunk.Depth - 1)]                 // going north - first block in chunk 0,-1 maps to 0,15
        [TestCase(0, -Chunk.Depth, 0, 0)]                     // going north - last block in chunk 0,-1 maps to 0,0
        [TestCase(0, -Chunk.Width -1, 0, Chunk.Width - 1)]    // going north - first block in chunk 0,-2 mapsto 0,15
        public void GlobalBlockToLocalBlock(int globalX, int globalZ, int localX, int localZ)
        {
            int y = (new Random()).Next(0, Chunk.Height);
            GlobalVoxelCoordinates global = new GlobalVoxelCoordinates(globalX, y, globalZ);
            LocalVoxelCoordinates expected = new LocalVoxelCoordinates(localX, y, localZ);
            LocalVoxelCoordinates actual;

            actual = (LocalVoxelCoordinates)global;

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.X, actual.X);
            Assert.AreEqual(expected.Y, actual.Y);
            Assert.AreEqual(expected.Z, actual.Z);
        }

        [TestCase(0, 0, 0, 0)]      // origin maps to origin
        [TestCase(Region.Width - 1, 0, Region.Width - 1, 0)]   // going east - last chunk in region 0, 0 maps to 31, 0
        [TestCase(Region.Width, 0, 0, 0)]                      // going east - first chunk in region 1, 0 maps to 0, 0
        [TestCase(0, Region.Depth - 1, 0, Region.Depth - 1)]   // going south - last chunk in region 0, 0 maps to 0, 31
        [TestCase(0, Region.Depth, 0, 0)]                      // going south - first chunk in region 1, 0 maps to 0, 0
        [TestCase(-1, 0, Region.Width - 1, 0)]                 // going west - first chunk in region -1, 0 maps to 31, 0
        [TestCase(-Region.Width, 0, 0, 0)]                     // going west - last chunk in region -1, 0 maps to 0, 0
        [TestCase(-Region.Width - 1, 0, 31, 0)]                // going west - first chunk in region -2, 0 maps to 31, 0
        [TestCase(0, -1, 0, Region.Depth - 1)]                 // going north - first chunk in region 0, -1 maps to 0, 31
        [TestCase(0, -Region.Depth, 0, 0)]                     // going north - last chunk in region 0, -1 maps to 0, 0
        [TestCase(0, -Region.Depth - 1, 0, Region.Depth - 1)]  // going north - first chunk in region 0, -2 maps to 0, 31
        public void GlobalChunkToLocalChunk_Test(int globalX, int globalZ, int localX, int localZ)
        {
            GlobalChunkCoordinates global = new GlobalChunkCoordinates(globalX, globalZ);
            LocalChunkCoordinates expected = new LocalChunkCoordinates(localX, localZ);
            LocalChunkCoordinates actual;

            actual = (LocalChunkCoordinates)global;

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.X, actual.X);
            Assert.AreEqual(expected.Z, actual.Z);
        }

        [TestCase(0, 0, 0, 0)]                   // origin maps to origin
        [TestCase(Region.Width - 1, 0, 0, 0)]    // going east - last chunk in region 0, 0
        [TestCase(Region.Width, 0, 1, 0)]        // going east - first chunk in region 1, 0
        [TestCase(-1, 0, -1, 0)]                 // going west - first chunk in region -1, 0
        [TestCase(-Region.Width, 0, -1, 0)]      // going west - last chunk in region -1, 0
        [TestCase(-Region.Width - 1, 0, -2, 0)]  // going west - first chunk in region -2, 0
        [TestCase(0, Region.Depth - 1, 0, 0)]    // going south - last  chunk in region 0, 0
        [TestCase(0, Region.Depth, 0, 1)]        // going south - first chunk in region 0, 1
        [TestCase(0, -Region.Depth, 0, -1)]      // going north - last  chunk in region 0, -1
        [TestCase(0, -Region.Depth - 1, 0, -2)]  // going north - first chunk in Region 0, -2
        public void GlobalChunkToRegion_Test(int globalX, int globalZ, int regionX, int regionZ)
        {
            GlobalChunkCoordinates global = new GlobalChunkCoordinates(globalX, globalZ);
            RegionCoordinates expected = new RegionCoordinates(regionX, regionZ);
            RegionCoordinates actual;

            actual = (RegionCoordinates)global;

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.X, actual.X);
            Assert.AreEqual(expected.Z, actual.Z);
        }

        [TestCase(0, 0, 0, 0)]                   // origin maps to origin
        [TestCase(Chunk.Width - 1, 0, 0, 0)]     // going east - last block in chunk 0, 0
        [TestCase(Chunk.Width, 0, 1, 0)]         // going east - first block in chunk 1, 0
        [TestCase(0, Chunk.Depth - 1, 0, 0)]     // going south - last block in chunk 0, 0
        [TestCase(0, Chunk.Depth, 0, 1)]         // going south - first block in chunk 0, 1
        [TestCase(-1, 0, -1, 0)]                 // going west - first block in chunk -1, 0
        [TestCase(-Chunk.Width, 0, -1, 0)]       // going west - last block in chunk -1, 0
        [TestCase(-Chunk.Width - 1, 0, -2, 0)]   // going west - first block in chunk -2, 0
        [TestCase(0, -1, 0, -1)]                 // going north - first block in chunk 0, -1
        [TestCase(0, -Chunk.Depth, 0, -1)]       // going north - last block in chunk 0, -1
        [TestCase(0, -Chunk.Depth - 1, 0, -2)]   // going north - first block in chunk 0, -2
        public void BlockToGlobalChunk_C2D_Test(int blockX, int blockZ, int globalX, int globalZ)
        {
            GlobalVoxelCoordinates global = new GlobalVoxelCoordinates(blockX, 0, blockZ);
            GlobalChunkCoordinates expected = new GlobalChunkCoordinates(globalX, globalZ);

            GlobalChunkCoordinates actual = (GlobalChunkCoordinates)global;

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.X, actual.X);
            Assert.AreEqual(expected.Z, actual.Z);
        }


        [TestCase(0, 0, 0, 0)]                   // origin maps to origin
        [TestCase(Chunk.Width - 1, 0, 0, 0)]     // going east - last block in chunk 0, 0
        [TestCase(Chunk.Width, 0, 1, 0)]         // going east - first block in chunk 1, 0
        [TestCase(0, Chunk.Depth - 1, 0, 0)]     // going south - last block in chunk 0, 0
        [TestCase(0, Chunk.Depth, 0, 1)]         // going south - first block in chunk 0, 1
        [TestCase(-1, 0, -1, 0)]                 // going west - first block in chunk -1, 0
        [TestCase(-Chunk.Width, 0, -1, 0)]       // going west - last block in chunk -1, 0
        [TestCase(-Chunk.Width - 1, 0, -2, 0)]   // going west - first block in chunk -2, 0
        [TestCase(0, -1, 0, -1)]                 // going north - first block in chunk 0, -1
        [TestCase(0, -Chunk.Depth, 0, -1)]       // going north - last block in chunk 0, -1
        [TestCase(0, -Chunk.Depth - 1, 0, -2)]   // going north - first block in chunk 0, -2
        public void BlockToGlobalChunk_V3_Test(double blockX, double blockZ, int globalX, int globalZ)
        {
            Vector3 global = new Vector3(blockX, 60, blockZ);
            Coordinates2D expected = new Coordinates2D(globalX, globalZ);
            Coordinates2D actual;

            actual = Coordinates.BlockToGlobalChunk(global);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.X, actual.X);
            Assert.AreEqual(expected.Z, actual.Z);
        }

        [TestCase(0, 0, 0, 0)]                                  // origin maps to origin
        [TestCase(Region.Width * Chunk.Width - 1, 0, 0, 0)]     // going east - last block in region 0, 0
        [TestCase(Region.Width * Chunk.Width, 0, 1, 0)]         // going east - first block in region 1, 0
        [TestCase(0, Region.Width * Chunk.Depth - 1, 0, 0)]     // going south - last block in region 0, 0
        [TestCase(0, Region.Width * Chunk.Depth, 0, 1)]         // going south - first block in region 0, 1
        [TestCase(-1, 0, -1, 0)]                                // going west - first block in region -1, 0
        [TestCase(-Region.Width * Chunk.Width, 0, -1, 0)]       // going west - last block in region -1, 0
        [TestCase(-Region.Width * Chunk.Width - 1, 0, -2, 0)]   // going west - first block in region -2, 0
        [TestCase(0, -1, 0, -1)]                                // going north - first block in region 0, -1
        [TestCase(0, -Region.Width * Chunk.Depth, 0, -1)]       // going north - last block in region 0, -1
        [TestCase(0, -Region.Width * Chunk.Depth - 1, 0, -2)]   // going north - first block in region 0, -2
        public void BlockToRegion_C2D_Test(int blockX, int blockZ, int regionX, int regionZ)
        {
            Coordinates2D global = new Coordinates2D(blockX, blockZ);
            Coordinates2D expected = new Coordinates2D(regionX, regionZ);
            Coordinates2D actual;

            actual = Coordinates.BlockToRegion(global);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.X, actual.X);
            Assert.AreEqual(expected.Z, actual.Z);
        }

        [TestCase(0, 0, 0, 0)]                                  // origin maps to origin
        [TestCase(Region.Width * Chunk.Width - 1, 0, 0, 0)]     // going east - last block in region 0, 0
        [TestCase(Region.Width * Chunk.Width, 0, 1, 0)]         // going east - first block in region 1, 0
        [TestCase(0, Region.Width * Chunk.Depth - 1, 0, 0)]     // going south - last block in region 0, 0
        [TestCase(0, Region.Width * Chunk.Depth, 0, 1)]         // going south - first block in region 0, 1
        [TestCase(-1, 0, -1, 0)]                                // going west - first block in region -1, 0
        [TestCase(-Region.Width * Chunk.Width, 0, -1, 0)]       // going west - last block in region -1, 0
        [TestCase(-Region.Width * Chunk.Width - 1, 0, -2, 0)]   // going west - first block in region -2, 0
        [TestCase(0, -1, 0, -1)]                                // going north - first block in region 0, -1
        [TestCase(0, -Region.Width * Chunk.Depth, 0, -1)]       // going north - last block in region 0, -1
        [TestCase(0, -Region.Width * Chunk.Depth - 1, 0, -2)]   // going north - first block in region 0, -2
        public void BlockToRegion_C3D_Test(int blockX, int blockZ, int regionX, int regionZ)
        {
            Coordinates3D global = new Coordinates3D(blockX, 60, blockZ);
            Coordinates2D expected = new Coordinates2D(regionX, regionZ);
            Coordinates2D actual;

            actual = Coordinates.BlockToRegion(global);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.X, actual.X);
            Assert.AreEqual(expected.Z, actual.Z);
        }


        [TestCase(0, 0, 0, 0)]                                               // origin maps to origin
        [TestCase(Region.Width * Chunk.Width - 1, 0, Region.Width - 1, 0)]   // going east - last block in region 0, 0
        [TestCase(Region.Width * Chunk.Width, 0, 0, 0)]                      // going east - first block in region 1, 0
        [TestCase(0, Region.Width * Chunk.Depth - 1, 0, Region.Depth - 1)]   // going south - last block in region 0, 0
        [TestCase(0, Region.Width * Chunk.Depth, 0, 0)]                      // going south - first block in region 0, 1
        [TestCase(-1, 0, Region.Width - 1, 0)]                               // going west - first block in region -1, 0
        [TestCase(-Region.Width * Chunk.Width, 0, 0, 0)]                     // going west - last block in region -1, 0
        [TestCase(-Region.Width * Chunk.Width - 1, 0, Region.Width - 1, 0)]  // going west - first block in region -2, 0
        [TestCase(0, -1, 0, Region.Depth - 1)]                               // going north - first block in region 0, -1
        [TestCase(0, -Region.Width * Chunk.Depth, 0, 0)]                     // going north - last block in region 0, -1
        [TestCase(0, -Region.Width * Chunk.Depth - 1, 0, Region.Depth - 1)]  // going north - first block in region 0, -2
        public void BlockToLocalChunk_C2D_Test(int blockX, int blockZ, int localX, int localZ)
        {
            Coordinates2D global = new Coordinates2D(blockX, blockZ);
            Coordinates2D expected = new Coordinates2D(localX, localZ);
            Coordinates2D actual;

            actual = Coordinates.BlockToLocalChunk(global);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.X, actual.X);
            Assert.AreEqual(expected.Z, actual.Z);
        }

        [TestCase(0, 0, 0, 0)]                                               // origin maps to origin
        [TestCase(Region.Width * Chunk.Width - 1, 0, Region.Width - 1, 0)]   // going east - last block in region 0, 0
        [TestCase(Region.Width * Chunk.Width, 0, 0, 0)]                      // going east - first block in region 1, 0
        [TestCase(0, Region.Width * Chunk.Depth - 1, 0, Region.Depth - 1)]   // going south - last block in region 0, 0
        [TestCase(0, Region.Width * Chunk.Depth, 0, 0)]                      // going south - first block in region 0, 1
        [TestCase(-1, 0, Region.Width - 1, 0)]                               // going west - first block in region -1, 0
        [TestCase(-Region.Width * Chunk.Width, 0, 0, 0)]                     // going west - last block in region -1, 0
        [TestCase(-Region.Width * Chunk.Width - 1, 0, Region.Width - 1, 0)]  // going west - first block in region -2, 0
        [TestCase(0, -1, 0, Region.Depth - 1)]                               // going north - first block in region 0, -1
        [TestCase(0, -Region.Width * Chunk.Depth, 0, 0)]                     // going north - last block in region 0, -1
        [TestCase(0, -Region.Width * Chunk.Depth - 1, 0, Region.Depth - 1)]  // going north - first block in region 0, -2
        public void BlockToLocalChunk_C3D_Test(int blockX, int blockZ, int localX, int localZ)
        {
            Coordinates3D global = new Coordinates3D(blockX, 63, blockZ);
            Coordinates2D expected = new Coordinates2D(localX, localZ);
            Coordinates2D actual;

            actual = Coordinates.BlockToLocalChunk(global);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.X, actual.X);
            Assert.AreEqual(expected.Z, actual.Z);
        }
    }
}
