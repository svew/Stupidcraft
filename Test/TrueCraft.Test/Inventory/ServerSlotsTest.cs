using System;
using System.Collections.Generic;
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
    public class SlotsTest
    {
        [Test]
        public void ctor()
        {
            Mock<IItemRepository> mock = new Mock<IItemRepository>(MockBehavior.Strict);
            int count = 56;
            IServerSlots slots = ServerSlots.GetServerSlots(mock.Object, count);

            Assert.AreEqual(count, slots.Count);
            for (int j = 0; j < count; j++)
                Assert.AreEqual(j, ((IServerSlot)slots[j]).Index);
        }

        [Test]
        public void GetPackets()
        {
            Mock<IItemRepository> mockRepo = new Mock<IItemRepository>(MockBehavior.Strict);
            ItemStack item1 = new ItemStack(56, 12, 1);
            int index1 = 17;
            ItemStack item2 = new ItemStack(42, 7, 0);
            int index2 = 19;
            int indexOffset = 5;
            IServerSlots slots = ServerSlots.GetServerSlots(mockRepo.Object, 27);
            slots[index1].Item = item1;
            slots[index2].Item = item2;
            sbyte windowID = 3;
            List<SetSlotPacket> packets = slots.GetSetSlotPackets(windowID, (short)indexOffset);

            Assert.AreEqual(2, packets.Count);

            Assert.AreEqual(windowID, packets[0].WindowID);
            Assert.AreEqual(indexOffset + index1, packets[0].SlotIndex);
            Assert.AreEqual(56, packets[0].ItemID);
            Assert.AreEqual(12, packets[0].Count);
            Assert.AreEqual(1, packets[0].Metadata);

            Assert.AreEqual(windowID, packets[1].WindowID);
            Assert.AreEqual(indexOffset + index2, packets[1].SlotIndex);
            Assert.AreEqual(42, packets[1].ItemID);
            Assert.AreEqual(7, packets[1].Count);
            Assert.AreEqual(0, packets[1].Metadata);
        }
    }
}
