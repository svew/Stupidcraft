using System;
using System.IO;
using System.Xml;
using fNbt;
using NUnit.Framework;


namespace TrueCraft.Core.Test
{
    [TestFixture]
    public class TestItemStack
    {
        // Test with default metadata
        [TestCase(17, 1, 0, 
            @"
          <c>
            <id> 17 </id>
            <count> 1 </count>
          </c>")]
        // Test with explicit metadata
        [TestCase(351, 1, 4, 
            @"<c>  
            <id>351</id>
            <count>1</count>
            <metadata>4</metadata>
          </c>")]
        // Test that Count is set to 0 for Empty Stacks
        [TestCase(-1, 0, 0, 
            @"<c>
            <id>-1</id>
            <count>1</count>
          </c>")]
        public void Test_Ctor_xml(short expectedID, sbyte expectedCount, short expectedMetadata, string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            ItemStack actual = new ItemStack(doc.FirstChild!);

            Assert.AreEqual(expectedID, actual.ID);
            Assert.AreEqual(expectedCount, actual.Count);
            Assert.AreEqual(expectedMetadata, actual.Metadata);
        }

        [TestCase(5)]
        [TestCase(-1)]
        public void Test_Ctor_id(short id)
        {
            ItemStack actual = new ItemStack(id);

            Assert.AreEqual(id, actual.ID);
            Assert.AreEqual(id != -1 ? 1 : 0, actual.Count);
            Assert.AreEqual(0, actual.Metadata);
        }

        [TestCase(3, 5)]
        [TestCase(-1, 11)]
        public void Test_Ctor_id_count(short id, sbyte count)
        {
            ItemStack actual = new ItemStack(id, count);

            Assert.AreEqual(id, actual.ID);
            Assert.AreEqual(id != -1 ? count : 0, actual.Count);
            Assert.AreEqual(0, actual.Metadata);
        }

        [TestCase(3, 5, 7)]
        public void Test_Ctor_id_count_metadata(short id, sbyte count, short metadata)
        {
            ItemStack actual = new ItemStack(id, count, metadata);

            Assert.AreEqual(id, actual.ID);
            Assert.AreEqual(count, actual.Count);
            Assert.AreEqual(metadata, actual.Metadata);
        }

        [TestCase(3, 5, 7, 1)]
        [TestCase(5, 7, 11, 2)]
        public void GetReducedItemStack(short id, sbyte count, short metadata, sbyte reduction)
        {
            ItemStack item = new ItemStack(id, count, metadata);

            ItemStack actual = item.GetReducedStack(reduction);

            Assert.AreEqual(id, actual.ID);
            Assert.AreEqual(count - reduction, actual.Count);
            Assert.AreEqual(metadata, actual.Metadata);
        }

        [Test]
        public void Equals_Nbt()
        {
            short id = 5;
            short metadata = 17;
            byte count = 19;
            byte slot = 23;

            NbtCompound compound = new NbtCompound();
            NbtShort idTag = new NbtShort("id", id);
            compound.Add(idTag);
            compound.Add(new NbtShort("Damage", metadata));
            compound.Add(new NbtByte("Count", count));
            compound.Add(new NbtByte("Slot", slot));

            ItemStack item = new ItemStack(id, (sbyte)count, metadata);
            item.Index = slot;

            Assert.True(item.Equals(compound));
            Assert.True(item == compound);
            Assert.False(item != compound);
            Assert.True(compound == item);
            Assert.False(compound != item);
        }
    }
}
