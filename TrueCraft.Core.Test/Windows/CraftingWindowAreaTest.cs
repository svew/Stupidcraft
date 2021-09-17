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

            Assert.AreEqual(StickItem.ItemID, area[0].ID);
            Assert.AreEqual(4, area[0].Count);
            Assert.False(area[1].Empty);
            Assert.True(area[2].Empty);
            Assert.False(area[3].Empty);
            Assert.True(area[4].Empty);
        }
    }
}