using System;
using TrueCraft.API.Logic;
using TrueCraft.API.Windows;
using TrueCraft.API;
using NUnit.Framework;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.Logic.Items;
using TrueCraft.Core.Windows;
using Moq;

namespace TrueCraft.Core.Test.Windows
{
    [TestFixture]
    public class CraftingWindowAreaTest
    {
        // NOTE: These tests depend upon an initialized CraftingRepository
        //  containing the recipe for sticks.

        [TestCase(2, 2)]
        [TestCase(3, 3)]
        public void ctor(int width, int height)
        {
            CraftingWindowContent actual = new CraftingWindowContent(CraftingRepository.Get(), width, height);

            Assert.AreEqual(1 + width * height, actual.Count);
            Assert.AreEqual(width, actual.Width);
            Assert.AreEqual(height, actual.Height);
        }

        [Test]
        public void Not_A_Recipe_2x2()
        {
            CraftingWindowContent area = new CraftingWindowContent(CraftingRepository.Get(), 2, 2);

            area[1] = new ItemStack(CobblestoneBlock.BlockID);

            Assert.Null(area.Recipe);
            Assert.True(area[0].Empty);
            Assert.False(area[1].Empty);
            Assert.True(area[2].Empty);
            Assert.True(area[3].Empty);
            Assert.True(area[4].Empty);
        }

        [Test]
        public void Is_A_Recipe_2x2()
        {
            CraftingWindowContent area = new CraftingWindowContent(CraftingRepository.Get(), 2, 2);
            ItemStack planks = new ItemStack(WoodenPlanksBlock.BlockID);

            area[1] = planks;
            area[3] = planks;

            Assert.NotNull(area[0]);
            Assert.AreEqual(StickItem.ItemID, area[0].ID);
            Assert.AreEqual(4, area[0].Count);
            Assert.False(area[1].Empty);
            Assert.True(area[2].Empty);
            Assert.False(area[3].Empty);
            Assert.True(area[4].Empty);
        }

        [Test]
        public void Not_A_Recipe_3x3()
        {
            CraftingWindowContent area = new CraftingWindowContent(CraftingRepository.Get(), 3, 3);

            area[7] = new ItemStack(StickItem.ItemID);

            Assert.Null(area.Recipe);
            Assert.True(area[0].Empty);
            for (int j = 1; j < area.Count; j++)
                Assert.True(j != 7 ? area[j].Empty : !area[j].Empty);
        }

        [Test]
        public void Is_A_Recipe_3x3()
        {
            CraftingWindowContent area = new CraftingWindowContent(CraftingRepository.Get(), 3, 3);
            ItemStack planks = new ItemStack(WoodenPlanksBlock.BlockID);

            area[3] = planks;
            area[6] = planks;

            Assert.NotNull(area[0]);
            Assert.AreEqual(StickItem.ItemID, area[0].ID);
            Assert.AreEqual(4, area[0].Count);

            Assert.True(area[1].Empty);
            Assert.True(area[2].Empty);
            Assert.False(area[3].Empty);
            Assert.True(area[4].Empty);
            Assert.True(area[5].Empty);
            Assert.False(area[6].Empty);
            Assert.True(area[7].Empty);
            Assert.True(area[8].Empty);
            Assert.True(area[9].Empty);
        }

        [Test]
        public void Is_Multiple_Recipes()
        {
            CraftingWindowContent area = new CraftingWindowContent(CraftingRepository.Get(),3, 3);
            ItemStack planks = new ItemStack(WoodenPlanksBlock.BlockID);

            // Enough items for two recipes.
            // Note there is an extra item in area[2].
            area[2] = new ItemStack(WoodenPlanksBlock.BlockID, 3);
            area[5] = new ItemStack(WoodenPlanksBlock.BlockID, 2);

            Assert.NotNull(area[0]);
            Assert.AreEqual(StickItem.ItemID, area[0].ID);
            Assert.AreEqual(8, area[0].Count);
        }

        [Test]
        public void TakeOutput1()
        {
            CraftingWindowContent area = new CraftingWindowContent(CraftingRepository.Get(), 2, 2);

            ItemStack planks = new ItemStack(WoodenPlanksBlock.BlockID, 1);
            area[1] = planks;
            area[3] = planks;

            // Take one output
            ItemStack actual = area.TakeOutput();

            // This output should be 4 sticks
            Assert.AreEqual(StickItem.ItemID, actual.ID);
            Assert.AreEqual(4, actual.Count);

            // The entire grid should now be empty.
            for (int j = 0; j < area.Count; j++)
                Assert.True(area[j].Empty);
        }

        [Test]
        public void TakeOutput2()
        {
            CraftingWindowContent area = new CraftingWindowContent(CraftingRepository.Get(), 2, 2);

            // Add enough for two recipes
            ItemStack planks = new ItemStack(WoodenPlanksBlock.BlockID, 2);
            area[1] = planks;
            area[3] = planks;

            // Take one output
            ItemStack actual = area.TakeOutput();

            // This output should be 4 sticks
            Assert.AreEqual(StickItem.ItemID, actual.ID);
            Assert.AreEqual(4, actual.Count);

            // There should still be input for one more recipe
            Assert.AreEqual(StickItem.ItemID, area[0].ID);
            Assert.AreEqual(4, area[0].Count);
            Assert.False(area[1].Empty);
            Assert.AreEqual(WoodenPlanksBlock.BlockID, area[1].ID);
            Assert.AreEqual(1, area[1].Count);
            Assert.True(area[2].Empty);
            Assert.False(area[3].Empty);
            Assert.AreEqual(WoodenPlanksBlock.BlockID, area[3].ID);
            Assert.AreEqual(1, area[3].Count);
            Assert.True(area[4].Empty);
        }

        // Put in items for a Pickaxe, hoe, and shovel.
        // Take them out one after the other.
        [Test]
        public void TakeOutput3()
        {
            CraftingWindowContent area = new CraftingWindowContent(CraftingRepository.Get(), 3, 3);

            // Add Cobblestone & sticks for a pickaxe, hoe, and shovel
            area[1] = new ItemStack(CobblestoneBlock.BlockID, 2);
            area[2] = new ItemStack(CobblestoneBlock.BlockID, 3);
            area[3] = new ItemStack(CobblestoneBlock.BlockID, 1);
            area[5] = new ItemStack(StickItem.ItemID, 3);
            area[8] = new ItemStack(StickItem.ItemID, 3);

            // Take out a pickaxe & confirm
            ItemStack ax = area.TakeOutput();
            Assert.AreEqual(StonePickaxeItem.ItemID, ax.ID);
            Assert.AreEqual(1, ax.Count);

            // Confirm remaining inputs
            Assert.AreEqual(1, area[1].Count);
            Assert.AreEqual(2, area[2].Count);
            Assert.True(area[3].Empty);
            Assert.AreEqual(2, area[5].Count);
            Assert.AreEqual(2, area[8].Count);

            // Take out a Hoe and confirm
            ItemStack hoe = area.TakeOutput();
            Assert.AreEqual(StoneHoeItem.ItemID, hoe.ID);
            Assert.AreEqual(1, hoe.Count);

            // confirm remaining inputs
            Assert.True(area[1].Empty);
            Assert.AreEqual(1, area[2].Count);
            Assert.True(area[3].Empty);
            Assert.AreEqual(1, area[5].Count);
            Assert.AreEqual(1, area[8].Count);

            // Take out a shovel & confirm
            ItemStack shovel = area.TakeOutput();
            Assert.AreEqual(StoneShovelItem.ItemID, shovel.ID);
            Assert.AreEqual(1, shovel.Count);

            // Confirm entire grid is now empty
            for (int j = 0; j < area.Count; j++)
                Assert.True(area[j].Empty);
        }
    }
}