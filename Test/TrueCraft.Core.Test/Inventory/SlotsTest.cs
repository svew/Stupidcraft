using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Inventory;

namespace TrueCraft.Core.Test.Inventory
{
    [TestFixture]
    public class SlotsTest
    {
        private static List<ISlot> GetSlots(IItemRepository itemRepository, int count)
        {
            List<ISlot> rv = new List<ISlot>(count);
            for (int j = 0; j < count; j++)
                rv.Add(new Slot(itemRepository));

            return rv;
        }

        [Test]
        public void ctor()
        {
            Mock<IItemRepository> mock = new Mock<IItemRepository>(MockBehavior.Strict);
            int count = 56;
            ISlots<ISlot> slots = new Slots<ISlot>(mock.Object, GetSlots(mock.Object, count));

            Assert.AreEqual(count, slots.Count);
        }

        [Test]
        public void TestIndexing()
        {
            Mock<IItemRepository> mock = new Mock<IItemRepository>(MockBehavior.Strict);

            int slotCount = 8;
            List<ISlot> slots = new List<ISlot>(slotCount);
            for (int j = 0; j < slotCount; j++)
                slots.Add(new Slot(mock.Object));

            ISlots<ISlot> area = new Slots<ISlot>(mock.Object, slots);

            // bounds checking
            Assert.Throws<ArgumentOutOfRangeException>(() => { var x = area[-1]; });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var x = area[slotCount]; });

            // Test storage and retrieval
            ItemStack area0Stack = new ItemStack(10);
            area[0].Item = area0Stack;

            ItemStack area1Stack = new ItemStack(20);
            area[1].Item = area1Stack; ;

            Assert.AreEqual(area0Stack, area[0].Item);
            Assert.AreEqual(area1Stack, area[1].Item);

            for (int j = 2; j < slotCount; j++)
                Assert.AreEqual(ItemStack.EmptyStack, area[j].Item);
        }


        // Test that a maximum stack size of 16 works
        [TestCase(
            new short[] { 0x14C, 0x14C, -1, -1 }, // 0x14C == snowball
            new sbyte[] { 14, 16, 0, 0 },
            0,
            new short[] { -1, 0x14C, -1, -1 },
            new sbyte[] { 0, 15, 0, 0 },
            0x14C, 15, false
            )]
        // Test that a maximum stack size of 64 works.
        [TestCase(
            new short[] { 1, -1, 1, -1 },   // 1 == StoneBlock
            new sbyte[] { 34, 0, 64, 0 },
            0,
            new short[] { -1, -1, 1, -1 },
            new sbyte[] { 0, 0, 62, 0 },
            1, 36, false
            )]
        // Test that topuponly works, with one potential target slot
        //   and leftover items
        [TestCase(
            new short[] { -1, -1, -1, 1 },
            new sbyte[] { 0, 0, 0, 64 },
            16,
            new short[] { -1, -1, -1, 1 },
            new sbyte[] { 0, 0, 0, 48 },
            1, 32, true
            )]
        // Test that topUpOnly works, with > 1 potential target slot
        //   and leftover items
        [TestCase(
            new short[] { -1, 1, -1, 1 },
            new sbyte[] { 0, 64, 0, 64 },
            8,
            new short[] { -1, 1, -1, 1 },
            new sbyte[] { 0, 48, 0, 56 },
            1, 32, true
            )]
        // Test with > 1 potential target slot
        //   More than one of the slots is needed to store the items
        //   and no leftover items
        [TestCase(
            new short[] { -1, 1, -1, 1 },
            new sbyte[] { 0, 64, 0, 63 },
            0,
            new short[] { -1, 1, -1, 1 },
            new sbyte[] { 0, 48, 0, 56 },
            1, 23, true
            )]
        // Test when there is more than one potential target slot
        //   and all items fit in the first potential target slot.
        [TestCase(
            new short[] { 1, 2, 1, -1 },
            new sbyte[] { 14, 62, 32, 0 },
            0,
            new short[] { 1, 2, 1, -1 },
            new sbyte[] { 12, 62, 32, 0 },
            1, 2, false
            )]
        // Test only part of the stack fits, in one potential target slot
        [TestCase(
            new short[] { 2, 2, 1, 2 },
            new sbyte[] { 1, 1, 64, 1 },
            29,
            new short[] { 2, 2, 1, 2 },
            new sbyte[] { 1, 1, 31, 1 },
            1, 62, false
            )]
        // Test when nothing fits
        [TestCase(
            new short[] { 2, 2, 2, 2 },
            new sbyte[] { 1, 1, 1, 1 },
            33,
            new short[] { 2, 2, 2, 2 },
            new sbyte[] { 1, 1, 1, 1 },
            1, 33, false
            )]
        // Test when inserting to an empty stack
        [TestCase(
            new short[] { 2, 2, 1, 2 },
            new sbyte[] { 1, 1, 17, 1 },
            0,
            new short[] { 2, 2, -1, 2 },
            new sbyte[] { 1, 1, 0, 1 },
            1, 17, false
            )]
        public void StoreItemStack(short[] expectedID, sbyte[] expectedCount,
            int expectedRemainCount,
            short[] contentID, sbyte[] contentCount,
            short addID, sbyte addCount, bool topUpOnly)
        {
            // basic check on input data
            Assert.True(expectedID.Length == expectedCount.Length &&
                expectedID.Length == contentID.Length &&
                expectedID.Length == contentCount.Length,
                "All arrays must have the same Length.");
            Assert.True(expectedRemainCount <= addCount, $"{nameof(expectedRemainCount)} must be less than or equal to {nameof(addCount)}");

            // TODO Mock Item Repository.
            IItemRepository itemRepository = ItemRepository.Get();

            // Setup
            List<ISlot> slots = new List<ISlot>(contentID.Length);
            for (int j = 0; j < contentID.Length; j++)
                slots.Add(new Slot(itemRepository));
            ISlots<ISlot> actual = new Slots<ISlot>(itemRepository, slots);
            for (int j = 0; j < contentID.Length; j++)
                actual[j].Item = new ItemStack(contentID[j], contentCount[j]);
            ItemStack add = new ItemStack(addID, addCount);

            // Action
            ItemStack remaining = actual.StoreItemStack(add, topUpOnly);

            // Assertions
            for (int j = 0; j < contentID.Length; j++)
            {
                Assert.AreEqual(expectedID[j], actual[j].Item.ID);
                Assert.AreEqual(expectedCount[j], actual[j].Item.Count);
            }
            if (expectedRemainCount == 0)
            {
                Assert.True(remaining.Empty);
            }
            else
            {
                Assert.AreEqual(expectedRemainCount, remaining.Count);
                Assert.AreEqual(addID, remaining.ID);
            }
        }
    }
}
