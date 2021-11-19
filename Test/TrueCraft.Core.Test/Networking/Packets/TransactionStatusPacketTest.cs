using System;
using System.IO;
using TrueCraft.Core.Networking;
using TrueCraft.Core.Networking.Packets;
using NUnit.Framework;

namespace TrueCraft.Core.Test.Networking.Packets
{
    [TestFixture]
    public class TransactionStatusPacketTest
    {
        [TestCase(0, 1, true)]
        [TestCase(3, 5, false)]
        public void ctor(sbyte windowID, short actionNumber, bool accepted)
        {
            TransactionStatusPacket actual = new TransactionStatusPacket(windowID, actionNumber, accepted);

            Assert.AreEqual(windowID, actual.WindowID);
            Assert.AreEqual(actionNumber, actual.ActionNumber);
            Assert.AreEqual(accepted, actual.Accepted);
        }

        private static byte[] ArgsToByteArray(sbyte windowID, short actionNumber, bool accepted)
        {
            byte[] rv = new byte[5];

            rv[0] = 0x6a;
            rv[1] = (byte)windowID;
            rv[2] = (byte)(actionNumber >> 8);
            rv[3] = (byte)(actionNumber & 0x00ff);
            rv[4] = (byte)(accepted ? 1 : 0);

            return rv;
        }

        [TestCase(0, 1, true)]
        [TestCase(3, 5, false)]
        public void WriteStream(sbyte windowID, short actionNumber, bool accepted)
        {
            byte[] expected = ArgsToByteArray(windowID, actionNumber, accepted);
            byte[] actual = new byte[5];
            TransactionStatusPacket packet = new TransactionStatusPacket(windowID, actionNumber, accepted);

            using (Stream strm = new MemoryStream(actual))
            {
                MinecraftStream mcStrm = new MinecraftStream(strm);
                mcStrm.WriteByte(0x6a);     // TODO: redo all packets to write their own IDs!!!!
                packet.WritePacket(mcStrm);
            }

            for (int j = 0; j < actual.Length; j++)
                Assert.AreEqual(expected[j], actual[j]);
        }

        [TestCase(0, 1, true)]
        [TestCase(3, 5, false)]
        public void ReadStream(sbyte windowID, short actionNumber, bool accepted)
        {
            byte[] inStream = ArgsToByteArray(windowID, actionNumber, accepted);
            TransactionStatusPacket actual = new TransactionStatusPacket(0, 0, false);

            using (Stream strm = new MemoryStream(inStream))
            {
                // Advance the input stream past the Packet ID.
                strm.Seek(1, SeekOrigin.Begin);
                actual.ReadPacket(new MinecraftStream(strm));
            }

            Assert.AreEqual(windowID, actual.WindowID);
            Assert.AreEqual(actionNumber, actual.ActionNumber);
            Assert.AreEqual(accepted, actual.Accepted);
        }
    }
}
