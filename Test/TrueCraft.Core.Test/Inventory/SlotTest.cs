using System;
using Moq;
using NUnit.Framework;
using TrueCraft.Core.Inventory;
using TrueCraft.Core.Logic;

namespace TrueCraft.Core.Test.Inventory
{
    [TestFixture]
    public class SlotTest
    {
        [Test]
        public void ctor()
        {
            Mock<IItemRepository> mock = new Mock<IItemRepository>(MockBehavior.Strict);
            ISlot slot = new Slot(mock.Object);

            Assert.AreEqual(ItemStack.EmptyStack, slot.Item);
        }

        [Test]
        public void ctor_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new Slot(null!));
        }

        [TestCase(280, 14, 0)]
        [TestCase(17, 12, 1)]
        public void Item(short itemID, sbyte itemCount, short itemMetadata)
        {
            Mock<IItemProvider> mockProvider = new Mock<IItemProvider>(MockBehavior.Strict);
            mockProvider.Setup((p) => p.MaximumStack).Returns(64);
            Mock<IItemRepository> mockRepo = new Mock<IItemRepository>(MockBehavior.Strict);
            mockRepo.Setup(m => m.GetItemProvider(It.IsAny<short>())).Returns(mockProvider.Object);

            ItemStack item = new ItemStack(itemID, itemCount, itemMetadata);

            ISlot slot = new Slot(mockRepo.Object);
            bool itemChanged = false;
            slot.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Item")
                    itemChanged = true;
            };

            slot.Item = item;

            Assert.True(itemChanged);
            Assert.AreEqual(item, slot.Item);

            itemChanged = false;
            slot.Item = item;

            Assert.False(itemChanged);
            Assert.AreEqual(item, slot.Item);
        }

        // Test adding to an empty slot.
        [TestCase(7, -1, 0, 0, 280, 7, 0)]
        // Test adding nothing to a slot
        [TestCase(0, 280, 7, 0, -1, 0, 0)]
        // Test adding incompatible item id
        [TestCase(0, 280, 14, 0, 14, 12, 0)]
        // Test adding incompatible metadata
        [TestCase(0, 0x107, 48, 1, 0x107, 8, 0)]
        // Test adding compatible item id (not full slot after)
        [TestCase(1, 280, 32, 0, 280, 1, 0)]
        // Test adding compatible item id (exactly full after)
        [TestCase(16, 0x107, 48, 0, 0x107, 16, 0)]
        // Test adding compatible item (too much to fit)
        [TestCase(8, 17, 56, 0, 17, 24, 0)]
        public void CanAccept(int expected, short itemID, sbyte itemCount, short itemMetadata,
            short addID, sbyte addCount, short addMetadata)
        {
            Mock<IItemProvider> mockProvider = new Mock<IItemProvider>(MockBehavior.Strict);
            mockProvider.Setup((p) => p.MaximumStack).Returns(64);
            Mock<IItemRepository> mockRepo = new Mock<IItemRepository>(MockBehavior.Strict);
            mockRepo.Setup(m => m.GetItemProvider(It.IsAny<short>())).Returns(mockProvider.Object);

            ItemStack item = new ItemStack(itemID, itemCount, itemMetadata);
            ItemStack add = new ItemStack(addID, addCount, addMetadata);

            ISlot slot = new Slot(mockRepo.Object);

            slot.Item = item;
            int canAccept = slot.CanAccept(add);

            Assert.AreEqual(expected, canAccept);
        }
    }
}