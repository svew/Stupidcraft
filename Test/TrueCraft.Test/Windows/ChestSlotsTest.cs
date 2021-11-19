using System;
using fNbt;
using Moq;
using NUnit.Framework;
using TrueCraft.Core.World;
using TrueCraft.Core.Windows;
using TrueCraft.Windows;

namespace TrueCraft.Core.Test.Windows
{
    [TestFixture]
    public class ChestSlotsTest
    {
        // Verify that world not being null is enforced
        [Test]
        public void ctor_null_world_throws()
        {
#if DEBUG
            GlobalVoxelCoordinates chest = new GlobalVoxelCoordinates(3, 1, 4);

            Assert.Throws<ArgumentNullException>(() => new ChestSlots(null, chest, null));
#else
             Assert.Pass();
#endif
        }

        // Verify that having the other half of the chest on
        // a different Y Level throws an ArgumentException.
        [Test]
        public void ctor_above_throws()
        {
#if DEBUG
            GlobalVoxelCoordinates chest = new GlobalVoxelCoordinates(3, 1, 4);
            GlobalVoxelCoordinates otherHalf = new GlobalVoxelCoordinates(3, 2, 5);
            Mock<IWorld> world = new Mock<IWorld>(MockBehavior.Strict);

            Assert.Throws<ArgumentException>(() => new ChestSlots(world.Object, chest, otherHalf));
#else
             Assert.Pass();
#endif
        }

        // Verify that having the other half of the chest too far away
        // in the x-direction throws ArgumentException.
        [Test]
        public void ctor_too_far_x_throws()
        {
#if DEBUG
            GlobalVoxelCoordinates chest = new GlobalVoxelCoordinates(3, 1, 4);
            GlobalVoxelCoordinates otherHalf = new GlobalVoxelCoordinates(5, 1, 4);
            Mock<IWorld> world = new Mock<IWorld>(MockBehavior.Strict);

            Assert.Throws<ArgumentException>(() => new ChestSlots(world.Object, chest, otherHalf));
#else
             Assert.Pass();
#endif
        }

        // Verify that having the other half of the chest too far away
        // in the z-direction throws ArgumentException.
        [Test]
        public void ctor_too_far_z_throws()
        {
#if DEBUG
            GlobalVoxelCoordinates chest = new GlobalVoxelCoordinates(3, 1, 4);
            GlobalVoxelCoordinates otherHalf = new GlobalVoxelCoordinates(3, 1, 2);
            Mock<IWorld> world = new Mock<IWorld>(MockBehavior.Strict);

            Assert.Throws<ArgumentException>(() => new ChestSlots(world.Object, chest, otherHalf));
#else
             Assert.Pass();
#endif
        }

        // Verify that the other half of the chest cannot be on top of the chest
        [Test]
        public void ctor_overlapping_throws()
        {
#if DEBUG
            GlobalVoxelCoordinates chest = new GlobalVoxelCoordinates(3, 1, 4);
            GlobalVoxelCoordinates otherHalf = new GlobalVoxelCoordinates(3, 1, 4);
            Mock<IWorld> world = new Mock<IWorld>(MockBehavior.Strict);

            Assert.Throws<ArgumentException>(() => new ChestSlots(world.Object, chest, otherHalf));
#else
             Assert.Pass();
#endif
        }

        private NbtCompound SingleChestTileEntity(int index, short id, sbyte cnt)
        {
            NbtCompound rv = new NbtCompound();
            NbtList items = new NbtList("Items", NbtTagType.Compound);
            rv.Add(items);

            ItemStack stack = new ItemStack(id, cnt);
            stack.Index = index;
            items.Add(stack.ToNbt());

            return rv;
        }

        [Test]
        public void ctor_single_chest()
        {
            GlobalVoxelCoordinates chest = new GlobalVoxelCoordinates(3, 1, 4);
            Mock<IWorld> world = new Mock<IWorld>(MockBehavior.Strict);
            int expectedSlotIndex = 13;
            short expectedId = 5;
            sbyte expectedCount = 2;
            NbtCompound chestContent = SingleChestTileEntity(expectedSlotIndex, expectedId, expectedCount);
            world.Setup<NbtCompound>((w) => w.GetTileEntity(chest)).Returns(chestContent);

            ChestSlots slots = new ChestSlots(world.Object, chest, null);

            Assert.AreEqual(ChestWindowConstants.ChestLength, slots.Count);
            Assert.AreEqual(ChestWindowConstants.ChestWidth, slots.Width);
            Assert.AreEqual(ChestWindowConstants.ChestHeight, slots.Height);

            for (int j = 0; j < ChestWindowConstants.ChestLength; j ++)
            {
                if (j == expectedSlotIndex)
                {
                    ItemStack stack = slots[j];
                    Assert.False(stack.Empty);
                    Assert.AreEqual(expectedId, stack.ID);
                    Assert.AreEqual(expectedCount, stack.Count);
                    Assert.AreEqual(0, stack.Metadata);
                    Assert.Null(stack.Nbt);
                }
                else
                {
                    Assert.True(slots[j].Empty);
                }
            }
        }

        [Test]
        public void ctor_double_chest()
        {
            GlobalVoxelCoordinates chest = new GlobalVoxelCoordinates(3, 1, 4);
            GlobalVoxelCoordinates otherHalf = new GlobalVoxelCoordinates(3, 1, 5);
            int expectedSlotIndex1 = 11;
            short expectedId1 = 3;
            sbyte expectedCount1 = 7;
            NbtCompound chestContent = SingleChestTileEntity(expectedSlotIndex1, expectedId1, expectedCount1);
            Mock<IWorld> world = new Mock<IWorld>(MockBehavior.Strict);
            world.Setup<NbtCompound>((w) => w.GetTileEntity(chest)).Returns(chestContent);
            int expectedSlotIndex2 = 17;
            short expectedId2 = 5;
            sbyte expectedCount2 = 11;
            NbtCompound otherHalfContent = SingleChestTileEntity(expectedSlotIndex2, expectedId2, expectedCount2);
            world.Setup<NbtCompound>((w) => w.GetTileEntity(otherHalf)).Returns(otherHalfContent);


            ChestSlots slots = new ChestSlots(world.Object, chest, otherHalf);

            Assert.AreEqual(2 * ChestWindowConstants.ChestLength, slots.Count);
            Assert.AreEqual(ChestWindowConstants.ChestWidth, slots.Width);
            Assert.AreEqual(2 * ChestWindowConstants.ChestHeight, slots.Height);

            for (int j = 0; j < 2 * ChestWindowConstants.ChestLength; j ++)
            {
                if (j == expectedSlotIndex1)
                {
                    ItemStack stack = slots[j];
                    Assert.False(stack.Empty);
                    Assert.AreEqual(expectedId1, stack.ID);
                    Assert.AreEqual(expectedCount1, stack.Count);
                    Assert.AreEqual(0, stack.Metadata);
                    Assert.Null(stack.Nbt);
                }
                else if (j == expectedSlotIndex2 + ChestWindowConstants.ChestLength)
                {
                    ItemStack stack = slots[j];
                    Assert.False(stack.Empty);
                    Assert.AreEqual(expectedId2, stack.ID);
                    Assert.AreEqual(expectedCount2, stack.Count);
                    Assert.AreEqual(0, stack.Metadata);
                    Assert.Null(stack.Nbt);
                }
                else
                {
                    Assert.True(slots[j].Empty);
                }
            }
        }

        [Test]
        public void setter()
        {
            // Setup
            // Note: Because otherHalf.X < chest.X, the constructor must swap them.
            GlobalVoxelCoordinates chest = new GlobalVoxelCoordinates(3, 1, 4);
            GlobalVoxelCoordinates otherHalf = new GlobalVoxelCoordinates(2, 1, 4);
            Mock<IWorld> world = new Mock<IWorld>(MockBehavior.Strict);
            world.Setup<NbtCompound>((w) => w.GetTileEntity(chest)).Returns((NbtCompound)null);
            world.Setup<NbtCompound>((w) => w.GetTileEntity(otherHalf)).Returns((NbtCompound)null);
            ItemStack item1 = new ItemStack(3, 5);
            int expectedIndex1 = 17;
            ItemStack item2 = new ItemStack(7, 11);
            int expectedIndex2 = 13;

            NbtCompound actualChestTileEntity = null;
            world.Setup((w) => w.SetTileEntity(chest, It.IsAny<NbtCompound>()))
                .Callback<GlobalVoxelCoordinates, NbtCompound>((loc, nbt) => actualChestTileEntity = nbt);

            NbtCompound actualOtherHalfTileEntity = null;
            world.Setup((w) => w.SetTileEntity(otherHalf, It.IsAny<NbtCompound>()))
                .Callback<GlobalVoxelCoordinates, NbtCompound>((loc, nbt) => actualOtherHalfTileEntity = nbt);

            // Act
            ChestSlots slots = new ChestSlots(world.Object, chest, otherHalf);
            slots[expectedIndex1 + ChestWindowConstants.ChestLength] = item1;
            slots[expectedIndex2] = item2;

            // Assert
            Assert.NotNull(actualChestTileEntity);
            NbtList items = (NbtList)actualChestTileEntity["Items"];
            Assert.NotNull(items);
            Assert.AreEqual(1, items.Count);
            ItemStack item = ItemStack.FromNbt((NbtCompound)items[0]);
            Assert.AreEqual(expectedIndex1, item.Index);
            Assert.AreEqual(item1.ID, item.ID);
            Assert.AreEqual(item1.Count, item.Count);
            Assert.AreEqual(item1.Metadata, item.Metadata);
            Assert.AreEqual(item1.Nbt, item.Nbt);

            Assert.NotNull(actualOtherHalfTileEntity);
            items = (NbtList)actualOtherHalfTileEntity["Items"];
            Assert.NotNull(items);
            Assert.AreEqual(1, items.Count);
            item = ItemStack.FromNbt((NbtCompound)items[0]);
            Assert.AreEqual(expectedIndex2, item.Index);
            Assert.AreEqual(item2.ID, item.ID);
            Assert.AreEqual(item2.Count, item.Count);
            Assert.AreEqual(item2.Metadata, item.Metadata);
            Assert.AreEqual(item2.Nbt, item.Nbt);
        }
    }
}
