using System;
using System.Diagnostics;
using Moq;
using NUnit.Framework;
using TrueCraft.Core.Lighting;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Test.World;
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
            IDimension dimension = new FakeDimension(GetBlockRepository());
            ILighter lighter = new OverWorldLighter(dimension, null!);
            IChunk chunk = dimension.GetChunk(GlobalChunkCoordinates.Zero)!;
            int ground = chunk.GetHeight(0, 0);

            lighter.DoLightingOperation(new LightingOperation(GlobalVoxelCoordinates.Zero, LightingOperationMode.Add, LightingOperationKind.Initial, 15));

            for (int y = ground + 5; y >= ground; y--)
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
                        GlobalVoxelCoordinates coords = new GlobalVoxelCoordinates(x, y, z);
                        int sky = dimension.GetSkyLight(coords);
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
            IDimension dimension = new FakeDimension(GetBlockRepository());
            IChunk chunk = dimension.GetChunk(GlobalChunkCoordinates.Zero)!;
            //var lighter = new Core.Lighting.Lighting(dimension, repository);
            ILighter lighter = new OverWorldLighter(dimension, null!);
            int xHole = 5;
            int zHole = 5;
            int yGround = chunk.GetHeight(xHole, zHole);

            dimension.SetBlockID(new GlobalVoxelCoordinates(xHole, yGround, zHole), 0);         // Create area that looks like so:
            dimension.SetBlockID(new GlobalVoxelCoordinates(xHole, yGround - 1, zHole), 0);     // x x  Light goes like so: |
            dimension.SetBlockID(new GlobalVoxelCoordinates(xHole, yGround - 2, zHole), 0);     // x x                      |
            dimension.SetBlockID(new GlobalVoxelCoordinates(xHole - 1, yGround - 2, zHole), 0); //   x                     -/

            lighter.DoLightingOperation(new LightingOperation(GlobalVoxelCoordinates.Zero, LightingOperationMode.Add, LightingOperationKind.Initial, 15));
            lighter.DoLightingOperation(new LightingOperation(new GlobalVoxelCoordinates(xHole, yGround - 2, zHole), LightingOperationMode.Add, LightingOperationKind.Sky, 15));

            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(xHole, yGround, zHole));
            Assert.AreEqual(15, dimension.GetSkyLight(new GlobalVoxelCoordinates(xHole, yGround, zHole)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(xHole, yGround - 1, zHole));
            Assert.AreEqual(15, dimension.GetSkyLight(new GlobalVoxelCoordinates(xHole, yGround - 1, zHole)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(xHole, yGround - 2, zHole));
            Assert.AreEqual(15, dimension.GetSkyLight(new GlobalVoxelCoordinates(5, yGround - 2, zHole)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(xHole - 1, yGround - 2, zHole));
            Assert.AreEqual(14, dimension.GetSkyLight(new GlobalVoxelCoordinates(xHole - 1, yGround - 2, zHole)));
        }

        [Test]
        public void TestFarPropegation()
        {
            var repository = GetBlockRepository();
            IDimension dimension = new FakeDimension(GetBlockRepository());
            IChunk chunk = dimension.GetChunk(GlobalChunkCoordinates.Zero)!;
            int xHole = 5;
            int zHole = 5;
            int yGround = chunk.GetHeight(xHole, zHole);
            ILighter lighter = new OverWorldLighter(dimension, null!);

            //lighter.InitialLighting(dimension.GetChunk(GlobalChunkCoordinates.Zero));

            dimension.SetBlockID(new GlobalVoxelCoordinates(xHole, yGround, zHole), 0);         // Create area that looks like so:
            dimension.SetBlockID(new GlobalVoxelCoordinates(xHole, yGround - 1, zHole), 0);     // x x  Light goes like so: |
            dimension.SetBlockID(new GlobalVoxelCoordinates(xHole, yGround - 2, zHole), 0);     // x x                      |
            dimension.SetBlockID(new GlobalVoxelCoordinates(xHole - 1, yGround - 2, zHole), 0); //   x                     -/

            for (int x = 0; x < 4; x++)
            {
                dimension.SetBlockID(new GlobalVoxelCoordinates(x, yGround - 2, zHole), 0); // Dig a tunnel
                // xxxxx x ish
                // x     x
                // xxxxxxx
            }

            lighter.DoLightingOperation(new LightingOperation(GlobalVoxelCoordinates.Zero, LightingOperationMode.Add, LightingOperationKind.Initial, 15));
            lighter.DoLightingOperation(new LightingOperation(new GlobalVoxelCoordinates(xHole, yGround - 2, zHole),
                LightingOperationMode.Add, LightingOperationKind.Sky, 15));

            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(xHole, yGround, zHole));
            Assert.AreEqual(15, dimension.GetSkyLight(new GlobalVoxelCoordinates(xHole, yGround, zHole)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(xHole, yGround - 1, zHole));
            Assert.AreEqual(15, dimension.GetSkyLight(new GlobalVoxelCoordinates(xHole, yGround - 1, zHole)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(xHole, yGround - 2, zHole));
            Assert.AreEqual(15, dimension.GetSkyLight(new GlobalVoxelCoordinates(xHole, yGround - 2, zHole)));

            byte expected = 15;
            for (int x = 5; x >= 0; x--)
            {
                Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(x, yGround - 2, zHole));
                Assert.AreEqual(expected, dimension.GetSkyLight(new GlobalVoxelCoordinates(x, yGround - 2, zHole)));
                expected--;
            }
        }

        [Test]
        public void TestFarPropegationx2()
        {
            IBlockRepository repository = GetBlockRepository();
            IDimension dimension = new FakeDimension(repository);
            IChunk chunk = dimension.GetChunk(GlobalChunkCoordinates.Zero)!;
            ILighter lighter = new OverWorldLighter(dimension, null!);
            int xHole = 5;
            int zHole = 5;
            int groundY = chunk.GetHeight(xHole, zHole);

            // Test this layout:
            // xxx x    y=3
            // x   x    y=2
            // x   x    y=1
            // xxxxx    y=0
            //
            //    ^ x,z = 5

            // Dig a hole 3 blocks deep at xHole,zHole.
            for (int y = groundY - 3; y <= groundY; y++)
                dimension.SetBlockID(new GlobalVoxelCoordinates(xHole, y, zHole), 0);

            // Dig a 1-wide, 2-high tunnel sideways for 4 blocks.
            for (int x = 1; x <= 4; x++) // Dig outwards
            {
                dimension.SetBlockID(new GlobalVoxelCoordinates(x + xHole, 2, zHole), 0);
                dimension.SetBlockID(new GlobalVoxelCoordinates(x + xHole, 1, zHole), 0);
            }

            var watch = new Stopwatch();
            watch.Start();

            lighter.DoLightingOperation(new LightingOperation(GlobalVoxelCoordinates.Zero, LightingOperationMode.Add, LightingOperationKind.Initial, 15));

            lighter.DoLightingOperation(new LightingOperation(new GlobalVoxelCoordinates(xHole, groundY - 3, zHole), LightingOperationMode.Add, LightingOperationKind.Sky, 15));

            watch.Stop();

            // Output lighting
            Console.WriteLine("Block IDS:");
            Console.WriteLine("y");
            for (int y = groundY; y >= groundY - 3; y--)
            {
                Console.Write($"{y} ");
                for (int x = 0; x <= 5; x++)
                {
                    Console.Write(dimension.GetBlockID(new GlobalVoxelCoordinates(x + xHole, y, zHole)).ToString("D2") + " ");
                }
                Console.WriteLine();
            }
            Console.Write("x:");
            for (int x = 0; x <= 5; x++)
                Console.Write($"{x + xHole:D2} ");
            Console.WriteLine();

            Console.WriteLine("Sky Light levels:");
            Console.WriteLine("y");
            for (int y = groundY; y >= groundY - 3; y--)
            {
                Console.Write($"{y} ");
                for (int x = 0; x <= 5; x++)
                {
                    Console.Write(dimension.GetSkyLight(new GlobalVoxelCoordinates(x + xHole, y, zHole)).ToString("D2") + " ");
                }
                Console.WriteLine();
            }

            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(xHole, groundY, zHole));
            Assert.AreEqual(15, dimension.GetSkyLight(new GlobalVoxelCoordinates(zHole, groundY, zHole)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(xHole, groundY- 1, zHole));
            Assert.AreEqual(15, dimension.GetSkyLight(new GlobalVoxelCoordinates(xHole, groundY - 1, zHole)));
            Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(xHole, groundY - 2, zHole));
            Assert.AreEqual(15, dimension.GetSkyLight(new GlobalVoxelCoordinates(xHole, groundY - 2, zHole)));

            byte expected = 15;
            for (int x = 5; x >= 0; x--)
            {
                Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(x + xHole, groundY - 1, zHole));
                Assert.AreEqual(expected, dimension.GetSkyLight(new GlobalVoxelCoordinates(x + xHole, groundY - 1, zHole)));
                expected--;
            }
            expected = 15;
            for (int x = 5; x >= 0; x--)
            {
                Console.WriteLine("Testing {0}", new GlobalVoxelCoordinates(x + xHole, groundY - 2, zHole));
                Assert.AreEqual(expected, dimension.GetSkyLight(new GlobalVoxelCoordinates(x + xHole, groundY - 2, zHole)));
                expected--;
            }

            Console.WriteLine("{0}ms", watch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Tests initial lighting with Sky Light 
        /// </summary>
        [Test]
        public void TestLeavesAndEtc()
        {
            IBlockRepository repository = GetBlockRepository();
            IDimension dimension = new FakeDimension(repository);
            IChunk chunk = dimension.GetChunk(GlobalChunkCoordinates.Zero)!;
            ILighter lighter = new OverWorldLighter(dimension, null!);
            int x = 5;
            int z = 5;
            int ground = chunk.GetHeight(x, z);

            for (int y = ground + 1; y <= ground + 16; y++)
            {
                GlobalVoxelCoordinates coords = new GlobalVoxelCoordinates(x, y, z);
                dimension.SetBlockID(coords, 0);
                dimension.SetBlockID(coords + Vector3i.East, DirtBlock.BlockID);
                dimension.SetBlockID(coords + Vector3i.West, DirtBlock.BlockID);
                dimension.SetBlockID(coords + Vector3i.North, DirtBlock.BlockID);
                dimension.SetBlockID(coords + Vector3i.South, DirtBlock.BlockID);
            }

            // Test this layout:
            // xox      o == leaves
            // x x      x == dirt
            // xox
            // x x
            // xox ...

            for (int y = ground + 1; y <= ground + 16; y++)
                if (y % 2 == 0)
                    dimension.SetBlockID(new GlobalVoxelCoordinates(x, y, z), LeavesBlock.BlockID);

            lighter.DoLightingOperation(new LightingOperation(GlobalVoxelCoordinates.Zero, LightingOperationMode.Add, LightingOperationKind.Initial, 15));

            // Output lighting
            for (int y = ground + 16; y >= ground; y--)
            {
                Console.Write(dimension.GetBlockID(new GlobalVoxelCoordinates(x, y, z)).ToString("D2"));
                Console.Write(" " + dimension.GetSkyLight(new GlobalVoxelCoordinates(x, y, z)).ToString("D2"));
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

            for (int y = ground + 16, i = 0; y >= ground; y--, i++)
            {
                byte ex;
                if (i < expected.Length)
                    ex = expected[i];
                else
                    ex = 0;
                Assert.AreEqual(ex, dimension.GetSkyLight(new GlobalVoxelCoordinates(x, y, z)));
            }
        }
    }
}

