using System;
using NUnit.Framework;
using TrueCraft.API.World;
using TrueCraft.Core.TerrainGen;
using TrueCraft.Core.Lighting;
using TrueCraft.Core.Logic;
using TrueCraft.API;
using TrueCraft.Core.World;
using TrueCraft.Core.Logic.Blocks;
using System.Diagnostics;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Test.Lighting
{
    [TestFixture]
    public class WorldLighterTest
    {
        private IBlockRepository GetBlockRepository()
        {
            // TODO Update to a Mock
            return BlockRepository.Get();
        }

        [Test]
        public void TestBasicLighting()
        {
            var repository = GetBlockRepository();
            var world = new TrueCraft.Core.World.World("TEST", new FlatlandGenerator());
            world.BlockRepository = repository;
            var lighter = new WorldLighting(world, repository);
            world.GetBlockID(GlobalVoxelCoordinates.Zero); // Generate a chunk
            lighter.InitialLighting(world.GetChunk(GlobalChunkCoordinates.Zero));

            for (int y = 5; y >= 0; y--)
            {
                Console.Write("Y: {0} ", y);
                Console.Write(world.GetBlockID(new GlobalVoxelCoordinates(0, y, 0)));
                Console.Write(" -> ");
                Console.WriteLine(world.GetSkyLight(new GlobalVoxelCoordinates(0, y, 0)));
            }

            // Validate behavior
            for (int y = 0; y < Chunk.Height; y++)
            {
                for (int x = 0; x < Chunk.Width; x++)
                {
                    for (int z = 0; z < Chunk.Depth; z++)
                    {
                        var coords = new GlobalVoxelCoordinates(x, y, z);
                        var sky = world.GetSkyLight(coords);
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
            var world = new TrueCraft.Core.World.World("TEST", new FlatlandGenerator());
            world.BlockRepository = repository;
            var lighter = new WorldLighting(world, repository);
            world.GetBlockID(GlobalVoxelCoordinates.Zero); // Generate a chunk
            lighter.InitialLighting(world.GetChunk(GlobalChunkCoordinates.Zero));

            world.SetBlockID(new GlobalVoxelCoordinates(5, 3, 5), 0); // Create area that looks like so:
            world.SetBlockID(new GlobalVoxelCoordinates(5, 2, 5), 0); // x x  Light goes like so: |
            world.SetBlockID(new GlobalVoxelCoordinates(5, 1, 5), 0); // x x                      |
            world.SetBlockID(new GlobalVoxelCoordinates(4, 1, 5), 0); //   x                     -/

            lighter.EnqueueOperation(new BoundingBox(new Vector3(5, 2, 5),
                new Vector3(6, 4, 6)), true);

            while (lighter.TryLightNext()) // Test lighting
            {
            }

            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(5, 3, 5));
            Assert.AreEqual(15, world.GetSkyLight(new GlobalVoxelCoordinates(5, 3, 5)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(5, 2, 5));
            Assert.AreEqual(15, world.GetSkyLight(new GlobalVoxelCoordinates(5, 2, 5)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(5, 1, 5));
            Assert.AreEqual(15, world.GetSkyLight(new GlobalVoxelCoordinates(5, 1, 5)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(4, 1, 5));
            Assert.AreEqual(14, world.GetSkyLight(new GlobalVoxelCoordinates(4, 1, 5)));
        }

        [Test]
        public void TestFarPropegation()
        {
            var repository = GetBlockRepository();
            var world = new TrueCraft.Core.World.World("TEST", new FlatlandGenerator());
            world.BlockRepository = repository;
            var lighter = new WorldLighting(world, repository);
            world.GetBlockID(GlobalVoxelCoordinates.Zero); // Generate a chunk
            lighter.InitialLighting(world.GetChunk(GlobalChunkCoordinates.Zero));

            world.SetBlockID(new GlobalVoxelCoordinates(5, 3, 5), 0); // Create area that looks like so:
            world.SetBlockID(new GlobalVoxelCoordinates(5, 2, 5), 0); // x x  Light goes like so: |
            world.SetBlockID(new GlobalVoxelCoordinates(5, 1, 5), 0); // x x                      |
            world.SetBlockID(new GlobalVoxelCoordinates(4, 1, 5), 0); //   x                     -/

            for (int x = 0; x < 4; x++)
            {
                world.SetBlockID(new GlobalVoxelCoordinates(x, 1, 5), 0); // Dig a tunnel
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
            Assert.AreEqual(15, world.GetSkyLight(new GlobalVoxelCoordinates(5, 3, 5)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(5, 2, 5));
            Assert.AreEqual(15, world.GetSkyLight(new GlobalVoxelCoordinates(5, 2, 5)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(5, 1, 5));
            Assert.AreEqual(15, world.GetSkyLight(new GlobalVoxelCoordinates(5, 1, 5)));

            byte expected = 15;
            for (int x = 5; x >= 0; x--)
            {
                Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(x, 1, 5));
                Assert.AreEqual(expected, world.GetSkyLight(new GlobalVoxelCoordinates(x, 1, 5)));
                expected--;
            }
        }

        [Test]
        public void TestFarPropegationx2()
        {
            var repository = GetBlockRepository();
            var world = new TrueCraft.Core.World.World("TEST", new FlatlandGenerator());
            world.BlockRepository = repository;
            var lighter = new WorldLighting(world, repository);
            world.GetBlockID(GlobalVoxelCoordinates.Zero); // Generate a chunk
            lighter.InitialLighting(world.GetChunk(GlobalChunkCoordinates.Zero));

            // Test this layout:
            // xxx x    y=3
            // x   x    y=2
            // x   x    y=1
            // xxxxx    y=0
            //
            //    ^ x,z = 5

            for (int y = 1; y <= 3; y++) // Dig hole
            {
                world.SetBlockID(new GlobalVoxelCoordinates(5, y, 5), 0);
            }

            for (int x = 0; x <= 4; x++) // Dig outwards
            {
                world.SetBlockID(new GlobalVoxelCoordinates(x, 2, 5), 0); // Dig a tunnel
                world.SetBlockID(new GlobalVoxelCoordinates(x, 1, 5), 0); // Dig a tunnel
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
                    Console.Write(world.GetBlockID(new GlobalVoxelCoordinates(x, y, 5)).ToString("D2") + " ");
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
                    Console.Write(world.GetSkyLight(new GlobalVoxelCoordinates(x, y, 5)).ToString("D2") + " ");
                }
                Console.WriteLine();
            }

            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(5, 3, 5));
            Assert.AreEqual(15, world.GetSkyLight(new GlobalVoxelCoordinates(5, 3, 5)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(5, 2, 5));
            Assert.AreEqual(15, world.GetSkyLight(new GlobalVoxelCoordinates(5, 2, 5)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(5, 1, 5));
            Assert.AreEqual(15, world.GetSkyLight(new GlobalVoxelCoordinates(5, 1, 5)));

            byte expected = 15;
            for (int x = 5; x >= 0; x--)
            {
                Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(x, 2, 5));
                Assert.AreEqual(expected, world.GetSkyLight(new GlobalVoxelCoordinates(x, 2, 5)));
                expected--;
            }
            expected = 15;
            for (int x = 5; x >= 0; x--)
            {
                Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(x, 1, 5));
                Assert.AreEqual(expected, world.GetSkyLight(new GlobalVoxelCoordinates(x, 1, 5)));
                expected--;
            }

            Console.WriteLine("{0}ms", watch.ElapsedMilliseconds);
        }

        [Test]
        public void TestLeavesAndEtc()
        {
            var repository = GetBlockRepository();
            var world = new TrueCraft.Core.World.World("TEST", new FlatlandGenerator());
            world.BlockRepository = repository;
            var lighter = new WorldLighting(world, repository);
            world.GetBlockID(GlobalVoxelCoordinates.Zero); // Generate a chunk

            for (int y = 1; y <= 16; y++)
            {
                var coords = new GlobalVoxelCoordinates(5, y, 5);
                world.SetBlockID(coords, 0);
                world.SetBlockID(coords + Vector3i.East, DirtBlock.BlockID);
                world.SetBlockID(coords + Vector3i.West, DirtBlock.BlockID);
                world.SetBlockID(coords + Vector3i.North, DirtBlock.BlockID);
                world.SetBlockID(coords + Vector3i.South, DirtBlock.BlockID);
            }
            world.GetChunk(GlobalChunkCoordinates.Zero).UpdateHeightMap();

            lighter.InitialLighting(world.GetChunk(GlobalChunkCoordinates.Zero));

            // Test this layout:
            // xox      o == leaves
            // x x
            // xox
            // x x
            // xox ...

            for (int y = 1; y <= 16; y++)
            {
                if (y % 2 == 1)
                    world.SetBlockID(new GlobalVoxelCoordinates(5, y, 5), LeavesBlock.BlockID);
            }
            world.GetChunk(GlobalChunkCoordinates.Zero).UpdateHeightMap();

            lighter.EnqueueOperation(new BoundingBox(new Vector3(5, 0, 5),
                    new Vector3(6, 16, 6)), true);

            while (lighter.TryLightNext()) // Test lighting
            {
            }

            // Output lighting
            for (int y = 16; y >= 0; y--)
            {
                Console.Write(world.GetBlockID(new GlobalVoxelCoordinates(5, y, 5)).ToString("D2"));
                Console.Write(" " + world.GetSkyLight(new GlobalVoxelCoordinates(5, y, 5)).ToString("D2"));
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
                Assert.AreEqual(ex, world.GetSkyLight(new GlobalVoxelCoordinates(5, y, 5)));
            }
        }
    }
}

