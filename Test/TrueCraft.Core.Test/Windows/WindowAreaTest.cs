using System;
using NUnit.Framework;
using TrueCraft.Core.Windows;
using TrueCraft.API;

namespace TrueCraft.Core.Test.Windows
{
    [TestFixture]
    public class WindowAreaTest
    {
        [Test]
        public void TestIndexing()
        {
            var area = new Slots(10, 10, 1);
            area[0] = new ItemStack(10);
            Assert.AreEqual(new ItemStack(10), area[0]);
            area[1] = new ItemStack(20);
            Assert.AreEqual(new ItemStack(20), area[1]);

            area[0] = ItemStack.EmptyStack;
        }
    }
}

