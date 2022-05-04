using System;
using fNbt;
using NUnit.Framework;
using TrueCraft.Core;

namespace TrueCraft.Core.Test
{
    [TestFixture]
    public class TestNybbleArray
    {
        public TestNybbleArray()
        {
        }

        [Test]
        public void ctor()
        {
            byte[] data = new byte[1024];

            NybbleArray actual = new NybbleArray(data, 0, data.Length);

            Assert.AreEqual(actual.Length, data.Length);
        }

        [Test]
        public void Indexer()
        {
            byte[] data = new byte[512];
            int offset = 128;
            int length = 64;

            NybbleArray actual = new NybbleArray(data, offset, length);

            for (int j = 0, jul = actual.Length; j < jul; j++)
                actual[j] = (byte)(j & 0x0f);

            for (int j = 0, jul = actual.Length; j < jul; j++)
                Assert.AreEqual(j & 0x0f, actual[j]);
        }

        [Test]
        public void Indexer_Throws()
        {
            byte[] data = new byte[1024];
            int offset = 32;
            int length = 128;

            NybbleArray actual = new NybbleArray(data, offset, length);

            // Getter
            Assert.Throws<IndexOutOfRangeException>(() => { byte b = actual[-1]; });
            Assert.DoesNotThrow(() => { byte b = actual[0]; });
            Assert.Throws<IndexOutOfRangeException>(() => { byte b = actual[actual.Length]; });
            Assert.DoesNotThrow(() => { byte b = actual[actual.Length - 1]; });

            // Setter
            Assert.Throws<IndexOutOfRangeException>(() => { actual[-1] = 0x0f; });
            Assert.DoesNotThrow(() => { actual[0] = 0x0e; });
            Assert.Throws<IndexOutOfRangeException>(() => { actual[actual.Length] = 0x0d; });
            Assert.DoesNotThrow(() => { actual[actual.Length - 1] = 0x0c; });
        }

        [Test]
        public void ToArray()
        {
            byte[] data = new byte[1024];
            int offset = 17;
            int length = 64;
            for (int j = 0; j < data.Length; j ++)
            {
                int v = j & 0x0f;
                v = (v << 4) | v;
                data[j] = (byte)v;
            }

            NybbleArray actual = new NybbleArray(data, offset, 2 * length);

            byte[] array = actual.ToArray();

            Assert.AreEqual(length, array.Length);
            for (int j = 0; j < array.Length; j ++)
                Assert.AreEqual(data[j + offset], array[j]);
        }

        [Test]
        public void Serialize()
        {
            byte[] data = new byte[512];
            int offset = 0;
            int length = data.Length;
            string tagName = "test";

            for (int k = 0; k < 2 * data.Length; k += 2)
                data[k / 2] = (byte)((k & 0x0f) | (((k + 1) & 0x0f) << 4));

            NybbleArray array = new NybbleArray(data, offset, 2 * length);

            NbtTag actual = array.Serialize(tagName);
            Assert.NotNull(actual);

            NbtByteArray? byteArray = actual as NbtByteArray;
            Assert.NotNull(byteArray);

            Assert.AreEqual(tagName, byteArray?.Name);
            Assert.AreEqual(length, byteArray?.ByteArrayValue.Length);

            for (int j = 0; j < data.Length; j++)
                Assert.AreEqual(data[j + offset], byteArray?.ByteArrayValue[j]);
        }

        [Test]
        public void Deserialize()
        {
            byte[] data = new byte[256];
            int length = data.Length;
            string tagName = "Fred";
            for (int j = 0; j < length; j++)
                data[j] = (byte)(j & 0x00ff);

            NbtByteArray byteArray = new NbtByteArray(tagName, data);

            NybbleArray actual = new NybbleArray(new byte[1], 0, 0);
            actual.Deserialize(byteArray);

            Assert.AreEqual(2 * length, actual.Length);
            for (int j = 0; j < actual.Length; j += 2)
                Assert.AreEqual(j & 0x0f, actual[j]);
            for (int j = 1; j < actual.Length; j += 2)
                Assert.AreEqual(j >> 4, actual[j]);
        }
    }
}
