using System;
using NUnit.Framework;
using Moq;
using Moq.Protected;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Server;
using TrueCraft.Core.World;
using TrueCraft.Core;
using TrueCraft.TerrainGen;

namespace TrueCraft.Test.Logic
{
    // TODO: This unit test depends upon the FlatlandGenerator
    [TestFixture]
    public class BlockProviderTest
    {
        private const int _testSeed = 314159;

        private readonly Mock<IDimension> _dimension;
        private readonly Mock<IMultiplayerServer> _server;
        private readonly Mock<IEntityManager> _entityManager;
        private readonly Mock<IRemoteClient> _user;
        private readonly Mock<IBlockRepository> _blockRepository;

        public BlockProviderTest()
        {
            _dimension = new Mock<IDimension>();
            _server = new Mock<IMultiplayerServer>();
            _entityManager = new Mock<IEntityManager>();
            _user = new Mock<IRemoteClient>();
            _blockRepository = new Mock<IBlockRepository>();

            _user.SetupGet(u => u.Dimension).Returns(_dimension.Object);
            _user.SetupGet(u => u.Server).Returns(_server.Object);

            _dimension.Setup(w => w.SetBlockID(It.IsAny<GlobalVoxelCoordinates>(), It.IsAny<byte>()));
            _dimension.Setup(d => d.BlockRepository).Returns(_blockRepository.Object);

            _server.SetupGet(s => s.BlockRepository).Returns(_blockRepository.Object);

            _entityManager.Setup(m => m.SpawnEntity(It.IsAny<IEntity>()));
        }

        protected void ResetMocks()
        {
            _dimension.Invocations.Clear();
            _server.Invocations.Clear();
            _entityManager.Invocations.Clear();
            _user.Invocations.Clear();
        }

        [Test]
        public void TestBlockMined()
        {
            ResetMocks();
            var blockProvider = new Mock<BlockProvider> { CallBase = true };
            var descriptor = new BlockDescriptor
            {
                ID = 10,
                Coordinates = GlobalVoxelCoordinates.Zero
            };

            blockProvider.Object.BlockMined(descriptor, BlockFace.PositiveY, _dimension.Object, _user.Object);
            _entityManager.Verify(m => m.SpawnEntity(It.Is<ItemEntity>(e => e.Item.ID == 10)));
            _dimension.Verify(w => w.SetBlockID(GlobalVoxelCoordinates.Zero, 0));

            blockProvider.Protected().Setup<ItemStack[]>("GetDrop", ItExpr.IsAny<BlockDescriptor>(), ItExpr.IsAny<ItemStack>())
                .Returns(() => new[] { new ItemStack(12) });
            blockProvider.Object.BlockMined(descriptor, BlockFace.PositiveY, _dimension.Object, _user.Object);
            _entityManager.Verify(m => m.SpawnEntity(It.Is<ItemEntity>(e => e.Item.ID == 12)));
            _dimension.Verify(w => w.SetBlockID(GlobalVoxelCoordinates.Zero, 0));
        }

        [Test]
        public void TestSupport()
        {
            // We need an actual world for this
            IDimension dimension = new TrueCraft.World.Dimension(string.Empty, DimensionID.Overworld, new FlatlandGenerator(_testSeed), _blockRepository.Object);
            dimension.SetBlockID(GlobalVoxelCoordinates.Zero, 1);
            GlobalVoxelCoordinates oneY = new GlobalVoxelCoordinates(0, 1, 0);
            dimension.SetBlockID(oneY, 2);

            var blockProvider = new Mock<BlockProvider> { CallBase = true };
            var updated = new BlockDescriptor { ID = 2, Coordinates = oneY };
            var source = new BlockDescriptor { ID = 2, Coordinates = new GlobalVoxelCoordinates(1, 0, 0) };
            blockProvider.Setup(b => b.GetSupportDirection(It.IsAny<BlockDescriptor>())).Returns(Vector3i.Down);

            var supportive = new Mock<IBlockProvider>();
            supportive.SetupGet(p => p.Opaque).Returns(true);
            var unsupportive = new Mock<IBlockProvider>();
            unsupportive.SetupGet(p => p.Opaque).Returns(false);

            _blockRepository.Setup(r => r.GetBlockProvider(It.Is<byte>(b => b == 1))).Returns(supportive.Object);
            _blockRepository.Setup(r => r.GetBlockProvider(It.Is<byte>(b => b == 3))).Returns(unsupportive.Object);

            blockProvider.Object.BlockUpdate(updated, source, _server.Object, dimension);
            _dimension.Verify(w => w.SetBlockID(oneY, 0), Times.Never);

            dimension.SetBlockID(GlobalVoxelCoordinates.Zero, 3);

            blockProvider.Object.BlockUpdate(updated, source, _server.Object, dimension);
            Assert.AreEqual(0, dimension.GetBlockID(oneY));
            _entityManager.Verify(m => m.SpawnEntity(It.Is<ItemEntity>(e => e.Item.ID == 2)));
        }
    }
}