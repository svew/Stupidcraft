using System;
using NUnit.Framework;
using TrueCraft.Core.Windows;

namespace TrueCraft.Core.Test.Windows
{
    [TestFixture]
    public class SlotsTest
    {
        public SlotsTest()
        {
        }

        [TestCase(5, 2, 2)]
        [TestCase(27, 9, 3)]
        public void ctor(int length, int width, int height)
        {
            Slots actual = new Slots(length, width, height);

            Assert.AreEqual(length, actual.Count);
            Assert.AreEqual(width, actual.Width);
            Assert.AreEqual(height, actual.Height);

            for (int j = 0; j < length; j++)
                Assert.True(actual[j].Empty);
        }

        [Test]
        public void ctor_throws()
        {
#if DEBUG
            Assert.Throws<ArgumentException>(() => new Slots(8, 3, 3));
#else
            Assert.Pass();
#endif
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

            // Setup
            Slots actual = new Slots(contentID.Length, contentID.Length, 1);
            for (int j = 0; j < contentID.Length; j++)
                actual[j] = new ItemStack(contentID[j], contentCount[j]);
            ItemStack add = new ItemStack(addID, addCount);

            // Action
            ItemStack remaining = actual.StoreItemStack(add, topUpOnly);

            // Assertions
            for (int j = 0; j < contentID.Length; j ++)
            {
                Assert.AreEqual(expectedID[j], actual[j].ID);
                Assert.AreEqual(expectedCount[j], actual[j].Count);
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
