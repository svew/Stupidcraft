using System;
using System.Diagnostics;
using Moq;
using NUnit.Framework;
using TrueCraft.Core.AI;
using TrueCraft.Core.Lighting;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;
using TrueCraft.TerrainGen;

namespace TrueCraft.Core.Test.AI
{
    [TestFixture]
    public class PathFindingTest
    {
        private const int _testSeed = 314159;

        private IBlockRepository _blockRepository;

        private ILightingQueue _lightingQueue;

        private IMultiplayerServer _server;

        private IEntityManager _entityManager;

        public PathFindingTest()
        {
            Mock<IBlockProvider> mockProvider = new Mock<IBlockProvider>(MockBehavior.Strict);
            mockProvider.Setup(x => x.ID).Returns(3);

            Mock<IBlockRepository> mockRepository = new Mock<IBlockRepository>(MockBehavior.Strict);
            mockRepository.Setup(x => x.GetBlockProvider(It.Is<byte>(b => b == 3))).Returns(mockProvider.Object);
            _blockRepository = mockRepository.Object;

            // For path-finding, we can safely ignore calls to the Lighting Queue.
            Mock<ILightingQueue> mockQueue = new Mock<ILightingQueue>();
            _lightingQueue = mockQueue.Object;

            Mock<IMultiplayerServer> mockServer = new Mock<IMultiplayerServer>(MockBehavior.Strict);
            _server = mockServer.Object;

            Mock<IEntityManager> mockEntityManager = new Mock<IEntityManager>(MockBehavior.Strict);
            _entityManager = mockEntityManager.Object;
        }

        private void DrawGrid(PathResult path, IDimension dimension)
        {
            for (int z = -8; z < 8; z++)
            {
                for (int x = -8; x < 8; x++)
                {
                    GlobalVoxelCoordinates coords = new GlobalVoxelCoordinates(x, 4, z);
                    if (path.Waypoints.Contains(coords))
                        Console.Write("o");
                    else
                    {
                        var id = dimension.GetBlockID(coords);
                        if (id != 0)
                            Console.Write("x");
                        else
                            Console.Write("_");
                    }
                }
                Console.WriteLine();
            }
        }

        private IDimension BuildDimension()
        {
            return new TrueCraft.World.Dimension(string.Empty, DimensionID.Overworld,
                _server, new FlatlandGenerator(_testSeed), _lightingQueue,
                _blockRepository, _entityManager);
        }

        [Test]
        public void TestAStarLinearPath()
        {
            IDimension dimension = BuildDimension();
            var astar = new AStarPathFinder();

            var watch = new Stopwatch();
            watch.Start();
            var path = astar.FindPath(dimension, new BoundingBox(),
               new GlobalVoxelCoordinates(0, 4, 0), new GlobalVoxelCoordinates(5, 4, 0));
            watch.Stop();
            DrawGrid(path, dimension);
            Console.WriteLine(watch.ElapsedMilliseconds + "ms");

            var expected = new[]
            {
                new GlobalVoxelCoordinates(0, 4, 0),
                new GlobalVoxelCoordinates(1, 4, 0),
                new GlobalVoxelCoordinates(2, 4, 0),
                new GlobalVoxelCoordinates(3, 4, 0),
                new GlobalVoxelCoordinates(4, 4, 0),
                new GlobalVoxelCoordinates(5, 4, 0)
            };
            for (int i = 0; i < path.Waypoints.Count; i++)
                Assert.AreEqual(expected[i], path.Waypoints[i]);
        }

        [Test]
        public void TestAStarDiagonalPath()
        {
            IDimension dimension = BuildDimension();
            var astar = new AStarPathFinder();
            GlobalVoxelCoordinates start = new GlobalVoxelCoordinates(0, 4, 0);
            GlobalVoxelCoordinates end = new GlobalVoxelCoordinates(5, 4, 5);

            var watch = new Stopwatch();
            watch.Start();
            var path = astar.FindPath(dimension, new BoundingBox(), start, end);
            watch.Stop();
            DrawGrid(path, dimension);
            Console.WriteLine(watch.ElapsedMilliseconds + "ms");

            // Just test the start and end, the exact results need to be eyeballed
            Assert.AreEqual(start, path.Waypoints[0]);
            Assert.AreEqual(end, path.Waypoints[path.Waypoints.Count - 1]);
        }

        [Test]
        public void TestAStarObstacle()
        {
            IDimension dimension = BuildDimension();
            var astar = new AStarPathFinder();
            GlobalVoxelCoordinates start = new GlobalVoxelCoordinates(0, 4, 0);
            GlobalVoxelCoordinates end = new GlobalVoxelCoordinates(5, 4, 0);
            dimension.SetBlockID(new GlobalVoxelCoordinates(3, 4, 0), 1); // Obstacle

            var watch = new Stopwatch();
            watch.Start();
            var path = astar.FindPath(dimension, new BoundingBox(), start, end);
            watch.Stop();
            DrawGrid(path, dimension);
            Console.WriteLine(watch.ElapsedMilliseconds + "ms");

            // Just test the start and end, the exact results need to be eyeballed
            Assert.AreEqual(start, path.Waypoints[0]);
            Assert.AreEqual(end, path.Waypoints[path.Waypoints.Count - 1]);
            Assert.IsFalse(path.Waypoints.Contains(new GlobalVoxelCoordinates(3, 4, 0)));
        }

        [Test]
        public void TestAStarImpossible()
        {
            IDimension dimension = BuildDimension();
            var astar = new AStarPathFinder();
            GlobalVoxelCoordinates start = new GlobalVoxelCoordinates(0, 4, 0);
            GlobalVoxelCoordinates end = new GlobalVoxelCoordinates(5, 4, 0);

            dimension.SetBlockID(start + Vector3i.East, 1);
            dimension.SetBlockID(start + Vector3i.West, 1);
            dimension.SetBlockID(start + Vector3i.North, 1);
            dimension.SetBlockID(start + Vector3i.South, 1);

            var watch = new Stopwatch();
            watch.Start();
            var path = astar.FindPath(dimension, new BoundingBox(), start, end);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds + "ms");

            Assert.IsNull(path);
        }

        [Test]
        public void TestAStarExitRoom()
        {
            IDimension dimension = BuildDimension();
            var astar = new AStarPathFinder();
            GlobalVoxelCoordinates start = new GlobalVoxelCoordinates(0, 4, 0);
            GlobalVoxelCoordinates end = new GlobalVoxelCoordinates(5, 4, 0);

            // North wall
            for (int x = -4; x < 4; x++)
                dimension.SetBlockID(new GlobalVoxelCoordinates(x, 4, -4), 1);
            // East wall
            for (int z = -4; z < 4; z++)
                dimension.SetBlockID(new GlobalVoxelCoordinates(3, 4, z), 1);
            // South wall
            for (int x = -4; x < 4; x++)
                dimension.SetBlockID(new GlobalVoxelCoordinates(x, 4, 4), 1);

            var watch = new Stopwatch();
            watch.Start();
            var path = astar.FindPath(dimension, new BoundingBox(), start, end);
            watch.Stop();
            DrawGrid(path, dimension);
            Console.WriteLine(watch.ElapsedMilliseconds + "ms");

            // Just test the start and end, the exact results need to be eyeballed
            Assert.AreEqual(start, path.Waypoints[0]);
            Assert.AreEqual(end, path.Waypoints[path.Waypoints.Count - 1]);
        }

        [Test]
        public void TestAStarAvoidRoom()
        {
            IDimension dimension = BuildDimension();
            var astar = new AStarPathFinder();
            GlobalVoxelCoordinates start = new GlobalVoxelCoordinates(-5, 4, 0);
            GlobalVoxelCoordinates end = new GlobalVoxelCoordinates(5, 4, 0);

            // North wall
            for (int x = -4; x < 4; x++)
                dimension.SetBlockID(new GlobalVoxelCoordinates(x, 4, -4), 1);
            // East wall
            for (int z = -4; z < 4; z++)
                dimension.SetBlockID(new GlobalVoxelCoordinates(3, 4, z), 1);
            // South wall
            for (int x = -4; x < 4; x++)
                dimension.SetBlockID(new GlobalVoxelCoordinates(x, 4, 4), 1);

            var watch = new Stopwatch();
            watch.Start();
            var path = astar.FindPath(dimension, new BoundingBox(), start, end);
            watch.Stop();
            DrawGrid(path, dimension);
            Console.WriteLine(watch.ElapsedMilliseconds + "ms");

            // Just test the start and end, the exact results need to be eyeballed
            Assert.AreEqual(start, path.Waypoints[0]);
            Assert.AreEqual(end, path.Waypoints[path.Waypoints.Count - 1]);
        }
    }
}