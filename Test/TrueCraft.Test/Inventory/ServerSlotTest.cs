using System;
using Moq;
using NUnit.Framework;
using TrueCraft.Core;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Networking.Packets;
using TrueCraft.Core.Server;
using TrueCraft.Inventory;

namespace TrueCraft.Test.Inventory
{
    [TestFixture]
    public class ServerSlotTest
    {
        [TestCase(2)]
        [TestCase(5)]
        public void ctor(int index)
        {
            Mock<IItemRepository> mock = new Mock<IItemRepository>(MockBehavior.Strict);
            IServerSlot slot = new ServerSlot(mock.Object, index);

            Assert.AreEqual(index, slot.Index);
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

            IServerSlot slot = new ServerSlot(mockRepo.Object, 42);
            bool dirtyChanged = false;
            slot.PropertyChanged += (s, e) =>
            {
               if (e.PropertyName == "Dirty")
                    dirtyChanged = true;
            };

            slot.Item = item;

            Assert.True(slot.Dirty);
            Assert.True(dirtyChanged);

            dirtyChanged = false;
            slot.Item = item;

            Assert.False(dirtyChanged);
            Assert.True(slot.Dirty);
        }

        [TestCase(0, 12, 280, 14, 0)]
        [TestCase(1, 7, -1, 0, 0)]
        public void GetSetSlotPacket(sbyte windowID, short index, short itemID, sbyte itemCount, short itemMetadata)
        {
            Mock<IItemRepository> mockRepo = new Mock<IItemRepository>(MockBehavior.Strict);
            ItemStack item = new ItemStack(itemID, itemCount, itemMetadata);
            IServerSlot slot = new ServerSlot(mockRepo.Object, index);

            slot.Item = item;

            bool dirtyChanged = false;
            slot.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Dirty")
                    dirtyChanged = true;
            };

            bool isDirty = slot.Dirty;
            SetSlotPacket packet = slot.GetSetSlotPacket(windowID);

            Assert.True((isDirty && dirtyChanged) || (!isDirty && !dirtyChanged));
            Assert.False(slot.Dirty);
            Assert.AreEqual(windowID, packet.WindowID);
            Assert.AreEqual(index, packet.SlotIndex);
            Assert.AreEqual(itemID, packet.ItemID);
            Assert.AreEqual(itemCount, packet.Count);
            Assert.AreEqual(itemMetadata, packet.Metadata);
        }
    }
}