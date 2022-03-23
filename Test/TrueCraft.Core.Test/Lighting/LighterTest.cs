using System;
using System.Diagnostics;
using Moq;
using NUnit.Framework;
using TrueCraft.Core.Lighting;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.World;

namespace TrueCraft.Core.Test.Lighting
{
    [TestFixture]
    public class LighterTest
    {
        private const int _testSeed = 314159;

        private IBlockRepository _blockRepository;

        public LighterTest()
        {
            Mock<IBlockProvider> mockProvider = new Mock<IBlockProvider>(MockBehavior.Strict);
            mockProvider.Setup(x => x.ID).Returns(3);

            Mock<IBlockRepository> mockRepository = new Mock<IBlockRepository>(MockBehavior.Strict);
            mockRepository.Setup(x => x.GetBlockProvider(It.Is<byte>(b => b == 3))).Returns(mockProvider.Object);
            _blockRepository = mockRepository.Object;
        }

        private IBlockRepository GetBlockRepository()
        {
            // TODO Update so that unit tests do not depend upon a global singleton.
            return BlockRepository.Get();
        }

        [Test]
        public void TestBasicLighting()
        {
            var repository = GetBlockRepository();
            IDimension dimension = new TrueCraft.Core.World.Dimension(string.Empty, DimensionID.Overworld, new FlatlandGenerator(_testSeed), _blockRepository);
            var lighter = new Core.Lighting.Lighting(dimension, repository);
            dimension.GetBlockID(GlobalVoxelCoordinates.Zero); // Generate a chunk
            lighter.InitialLighting(dimension.GetChunk(GlobalChunkCoordinates.Zero));

            for (int y = 5; y >= 0; y--)
            {
                Console.Write("Y: {0} ", y);
                Console.Write(dimension.GetBlockID(new GlobalVoxelCoordinates(0, y, 0)));
                Console.Write(" -> ");
                Console.WriteLine(dimension.GetSkyLight(new GlobalVoxelCoordinates(0, y, 0)));
            }

            // Validate behavior
            for (int y = 0; y < WorldConstants.Height; y++)
            {
                for (int x = 0; x < WorldConstants.ChunkWidth; x++)
                {
                    for (int z = 0; z < WorldConstants.ChunkDepth; z++)
                    {
                        var coords = new GlobalVoxelCoordinates(x, y, z);
                        var sky = dimension.GetSkyLight(coords);
                        if (y < 4)
                            Assert.AreEqual(0, sky, coords.ToString());
                        else
                            Assert.AreEqual(15, sky, coords.ToString());
                    }
                }
            }
        }

        [Test]
        public void TestShortPropegation()
        {
            var repository = GetBlockRepository();
            IDimension dimension = new TrueCraft.Core.World.Dimension(string.Empty, "TEST", new FlatlandGenerator(_testSeed), _blockRepository);
            var lighter = new Core.Lighting.Lighting(dimension, repository);
            dimension.GetBlockID(GlobalVoxelCoordinates.Zero); // Generate a chunk
            lighter.InitialLighting(dimension.GetChunk(GlobalChunkCoordinates.Zero));

            dimension.SetBlockID(new GlobalVoxelCoordinates(5, 3, 5), 0); // Create area that looks like so:
            dimension.SetBlockID(new GlobalVoxelCoordinates(5, 2, 5), 0); // x x  Light goes like so: |
            dimension.SetBlockID(new GlobalVoxelCoordinates(5, 1, 5), 0); // x x                      |
            dimension.SetBlockID(new GlobalVoxelCoordinates(4, 1, 5), 0); //   x                     -/

            lighter.EnqueueOperation(new BoundingBox(new Vector3(5, 2, 5),
                new Vector3(6, 4, 6)), true);

            while (lighter.TryLightNext()) // Test lighting
            {
            }

            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(5, 3, 5));
            Assert.AreEqual(15, dimension.GetSkyLight(new GlobalVoxelCoordinates(5, 3, 5)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(5, 2, 5));
            Assert.AreEqual(15, dimension.GetSkyLight(new GlobalVoxelCoordinates(5, 2, 5)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(5, 1, 5));
            Assert.AreEqual(15, dimension.GetSkyLight(new GlobalVoxelCoordinates(5, 1, 5)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(4, 1, 5));
            Assert.AreEqual(14, dimension.GetSkyLight(new GlobalVoxelCoordinates(4, 1, 5)));
        }

        [Test]
        public void TestFarPropegation()
        {
            var repository = GetBlockRepository();
            IDimension dimension = new TrueCraft.Core.World.Dimension(string.Empty, "TEST", new FlatlandGenerator(_testSeed), _blockRepository);
            var lighter = new Core.Lighting.Lighting(dimension, repository);
            dimension.GetBlockID(GlobalVoxelCoordinates.Zero); // Generate a chunk
            lighter.InitialLighting(dimension.GetChunk(GlobalChunkCoordinates.Zero));

            dimension.SetBlockID(new GlobalVoxelCoordinates(5, 3, 5), 0); // Create area that looks like so:
            dimension.SetBlockID(new GlobalVoxelCoordinates(5, 2, 5), 0); // x x  Light goes like so: |
            dimension.SetBlockID(new GlobalVoxelCoordinates(5, 1, 5), 0); // x x                      |
            dimension.SetBlockID(new GlobalVoxelCoordinates(4, 1, 5), 0); //   x                     -/

            for (int x = 0; x < 4; x++)
            {
                dimension.SetBlockID(new GlobalVoxelCoordinates(x, 1, 5), 0); // Dig a tunnel
                // xxxxx x ish
                // x     x
                // xxxxxxx
            }

            lighter.EnqueueOperation(new BoundingBox(new Vector3(5, 2, 5),
                    new Vector3(6, 4, 6)), true);

            while (lighter.TryLightNext()) // Test lighting
            {
            }

            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(5, 3, 5));
            Assert.AreEqual(15, dimension.GetSkyLight(new GlobalVoxelCoordinates(5, 3, 5)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(5, 2, 5));
            Assert.AreEqual(15, dimension.GetSkyLight(new GlobalVoxelCoordinates(5, 2, 5)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(5, 1, 5));
            Assert.AreEqual(15, dimension.GetSkyLight(new GlobalVoxelCoordinates(5, 1, 5)));

            byte expected = 15;
            for (int x = 5; x >= 0; x--)
            {
                Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(x, 1, 5));
                Assert.AreEqual(expected, dimension.GetSkyLight(new GlobalVoxelCoordinates(x, 1, 5)));
                expected--;
            }
        }

        [Test]
        public void TestFarPropegationx2()
        {
            var repository = GetBlockRepository();
            IDimension dimension = new TrueCraft.Core.World.Dimension(string.Empty, "TEST", new FlatlandGenerator(_testSeed), _blockRepository);
            var lighter = new Core.Lighting.Lighting(dimension, repository);
            dimension.GetBlockID(GlobalVoxelCoordinates.Zero); // Generate a chunk
            lighter.InitialLighting(dimension.GetChunk(GlobalChunkCoordinates.Zero));

            // Test this layout:
            // xxx x    y=3
            // x   x    y=2
            // x   x    y=1
            // xxxxx    y=0
            //
            //    ^ x,z = 5

            for (int y = 1; y <= 3; y++) // Dig hole
            {
                dimension.SetBlockID(new GlobalVoxelCoordinates(5, y, 5), 0);
            }

            for (int x = 0; x <= 4; x++) // Dig outwards
            {
                dimension.SetBlockID(new GlobalVoxelCoordinates(x, 2, 5), 0); // Dig a tunnel
                dimension.SetBlockID(new GlobalVoxelCoordinates(x, 1, 5), 0); // Dig a tunnel
            }

            var watch = new Stopwatch();
            watch.Start();

            lighter.EnqueueOperation(new BoundingBox(new Vector3(5, 2, 5),
                    new Vector3(6, 4, 6)), true);

            while (lighter.TryLightNext()) // Test lighting
            {
            }

            watch.Stop();

            // Output lighting
            Console.WriteLine("Block IDS:");
            Console.WriteLine("y");
            for (int y = 3; y >= 0; y--)
            {
                Console.Write($"{y} ");
                for (int x = 0; x <= 5; x++)
                {
                    Console.Write(dimension.GetBlockID(new GlobalVoxelCoordinates(x, y, 5)).ToString("D2") + " ");
                }
                Console.WriteLine();
            }
            Console.Write("x:");
            for (int x = 0; x <= 5; x++)
                Console.Write($"{x:D2} ");
            Console.WriteLine();

            Console.WriteLine("Sky Light levels:");
            Console.WriteLine("y");
            for (int y = 3; y >= 0; y--)
            {
                Console.Write($"{y} ");
                for (int x = 0; x <= 5; x++)
                {
                    Console.Write(dimension.GetSkyLight(new GlobalVoxelCoordinates(x, y, 5)).ToString("D2") + " ");
                }
                Console.WriteLine();
            }

            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(5, 3, 5));
            Assert.AreEqual(15, dimension.GetSkyLight(new GlobalVoxelCoordinates(5, 3, 5)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(5, 2, 5));
            Assert.AreEqual(15, dimension.GetSkyLight(new GlobalVoxelCoordinates(5, 2, 5)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(5, 1, 5));
            Assert.AreEqual(15, dimension.GetSkyLight(new GlobalVoxelCoordinates(5, 1, 5)));

            byte expected = 15;
            for (int x = 5; x >= 0; x--)
            {
                Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(x, 2, 5));
                Assert.AreEqual(expected, dimension.GetSkyLight(new GlobalVoxelCoordinates(x, 2, 5)));
                expected--;
            }
            expected = 15;
            for (int x = 5; x >= 0; x--)
            {
                Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(x, 1, 5));
                Assert.AreEqual(expected, dimension.GetSkyLight(new GlobalVoxelCoordinates(x, 1, 5)));
                expected--;
            }

            Console.WriteLine("{0}ms", watch.ElapsedMilliseconds);
        }

        [Test]
        public void TestLeavesAndEtc()
        {
            var repository = GetBlockRepository();
            IDimension dimension = new TrueCraft.Core.World.Dimension(string.Empty, "TEST", new FlatlandGenerator(_testSeed), _blockRepository);
            var lighter = new Core.Lighting.Lighting(dimension, repository);
            dimension.GetBlockID(GlobalVoxelCoordinates.Zero); // Generate a chunk

            for (int y = 1; y <= 16; y++)
            {
                var coords = new GlobalVoxelCoordinates(5, y, 5);
                dimension.SetBlockID(coords, 0);
                dimension.SetBlockID(coords + Vector3i.East, DirtBlock.BlockID);
                dimension.SetBlockID(coords + Vector3i.West, DirtBlock.BlockID);
                dimension.SetBlockID(coords + Vector3i.North, DirtBlock.BlockID);
                dimension.SetBlockID(coords + Vector3i.South, DirtBlock.BlockID);
            }
            dimension.GetChunk(GlobalChunkCoordinates.Zero).UpdateHeightMap();

            lighter.InitialLighting(dimension.GetChunk(GlobalChunkCoordinates.Zero));

            // Test this layout:
            // xox      o == leaves
            // x x
            // xox
            // x x
            // xox ...

            for (int y = 1; y <= 16; y++)
            {
                if (y % 2 == 1)
                    dimension.SetBlockID(new GlobalVoxelCoordinates(5, y, 5), LeavesBlock.BlockID);
            }
            dimension.GetChunk(GlobalChunkCoordinates.Zero).UpdateHeightMap();

            lighter.EnqueueOperation(new BoundingBox(new Vector3(5, 0, 5),
                    new Vector3(6, 16, 6)), true);

            while (lighter.TryLightNext()) // Test lighting
            {
            }

            // Output lighting
            for (int y = 16; y >= 0; y--)
            {
                Console.Write(dimension.GetBlockID(new GlobalVoxelCoordinates(5, y, 5)).ToString("D2"));
                Console.Write(" " + dimension.GetSkyLight(new GlobalVoxelCoordinates(5, y, 5)).ToString("D2"));
                Console.WriteLine("   Y={0}", y);
            }                

            var expected = new byte[]
            {
                15, // air
                13, // leaves
                12, // air
                10, // leaves
                9, // air
                7,  // leaves
                6,  // air
                4,  // leaves
                3,  // air
                1,  // leaves
                0,  // air
                0,  // leaves
            };

            for (int y = 16, i = 0; y >= 0; y--, i++)
            {
                byte ex;
                if (i < expected.Length)
                    ex = expected[i];
                else
                    ex = 0;
                Assert.AreEqual(ex, dimension.GetSkyLight(new GlobalVoxelCoordinates(5, y, 5)));
            }
        }
    }
}

