using System;
using TrueCraft.Core.Networking.Packets;
using NUnit.Framework;
using System.IO;
using TrueCraft.Core.Networking;

namespace TrueCraft.Core.Test.Networking.Packets
{
    [TestFixture]
    public class ChangeHeldItemPacketTest
    {
        [TestCase(5)]
        [TestCase(0)]
        public void Ctor(short slot)
        {
            ChangeHeldItemPacket actual = new ChangeHeldItemPacket(slot);

            Assert.AreEqual(slot, actual.Slot);
        }

        [TestCase(-1, false)]
        [TestCase(0, true)]
        [TestCase(8, true)]
        [TestCase(9, false)]
        public void ctor_throws(short slot, bool valid)
        {
#if DEBUG
            if (!valid)
                Assert.Throws<ArgumentOutOfRangeException>(() => new ChangeHeldItemPacket(slot));
            else
                Assert.DoesNotThrow(() => new ChangeHeldItemPacket(slot));
#else
            Assert.Pass();
#endif
        }


        private static byte[] ArgsToByteArray(short slot)
        {
            byte[] rv = new byte[3];

            rv[0] = 0x10;
            rv[1] = (byte)((slot >> 8) & 0x00ff);
            rv[2] = (byte)(slot & 0x00ff);

            return rv;
        }

        [TestCase(0)]
        [TestCase(7)]
        public void WriteStream(short slot)
        {
            byte[] expected = ArgsToByteArray(slot);
            byte[] actual = new byte[3];
            ChangeHeldItemPacket packet = new ChangeHeldItemPacket(slot);

            using (Stream strm = new MemoryStream(actual))
            {
                MinecraftStream mcStrm = new MinecraftStream(strm);
                mcStrm.WriteByte(0x10);     // TODO: redo all packets to write their own IDs!!!!
                packet.WritePacket(mcStrm);
            }

            for (int j = 0; j < actual.Length; j++)
                Assert.AreEqual(expected[j], actual[j]);
        }


        [TestCase(0)]
        [TestCase(7)]
        public void ReadStream(short slot)
        {
            byte[] inStream = ArgsToByteArray(slot);
            ChangeHeldItemPacket actual = new ChangeHeldItemPacket(slot);

            using (Stream strm = new MemoryStream(inStream))
            {
                // Advance the input stream past the Packet ID.
                strm.Seek(1, SeekOrigin.Begin);
                actual.ReadPacket(new MinecraftStream(strm));
            }

            Assert.AreEqual(slot, actual.Slot);
        }
    }
}
