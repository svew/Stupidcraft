using System;

namespace TrueCraft.Core.Networking.Packets
{
    /// <summary>
    /// Sent by the server to inform the client if an inventory transaction was successful.
    /// </summary>
    public struct TransactionStatusPacket : IPacket
    {
        public byte ID { get { return 0x6A; } }

        public sbyte WindowID;
        public short ActionNumber;
        public bool Accepted;

        public TransactionStatusPacket(sbyte windowID, short actionNumber, bool accepted)
        {
            WindowID = windowID;
            ActionNumber = actionNumber;
            Accepted = accepted;
        }

        public void ReadPacket(IMinecraftStream stream)
        {
            WindowID = stream.ReadInt8();
            ActionNumber = stream.ReadInt16();
            Accepted = stream.ReadBoolean();
        }

        public void WritePacket(IMinecraftStream stream)
        {
            stream.WriteInt8(WindowID);
            stream.WriteInt16(ActionNumber);
            stream.WriteBoolean(Accepted);
        }
    }
}