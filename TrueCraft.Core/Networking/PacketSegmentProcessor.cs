using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using TrueCraft.Core.Collections;

namespace TrueCraft.Core.Networking
{
    public class PacketSegmentProcessor : IPacketSegmentProcessor
    {

        public List<byte> PacketBuffer { get; private set; }

        public PacketReader PacketReader { get; protected set; }

        public bool ServerBound { get; private set; }
        
        public IPacket? CurrentPacket { get; protected set; }

        public PacketSegmentProcessor(PacketReader packetReader, bool serverBound)
        {
            PacketBuffer = new List<byte>();
            PacketReader = packetReader;
            ServerBound = serverBound;
            CurrentPacket = null;
        }

        public bool ProcessNextSegment(byte[] nextSegment, int offset, int len, [MaybeNullWhen(false)] out IPacket packet)
        {
            packet = null;
            CurrentPacket = null;

            if (nextSegment.Length > 0)
            {
                PacketBuffer.AddRange(new ByteArraySegment(nextSegment, offset, len));
            }

            if (PacketBuffer.Count == 0)
                return false;
            
            if (CurrentPacket is null)
            {
                byte packetId = PacketBuffer[0];

                Func<IPacket> createPacket;
                if (ServerBound)
                    createPacket = PacketReader.ServerboundPackets[packetId];
                else
                    createPacket = PacketReader.ClientboundPackets[packetId];

                if (createPacket is null)
                    throw new NotSupportedException("Unable to read packet type 0x" + packetId.ToString("X2"));

                CurrentPacket = createPacket();
            }
            
            using (ByteListMemoryStream listStream = new ByteListMemoryStream(PacketBuffer, 1))
            {
                using (MinecraftStream ms = new MinecraftStream(listStream))
                {
                    try
                    {
                        CurrentPacket.ReadPacket(ms);
                    }
                    catch (EndOfStreamException)
                    {
                        return false;
                    }
                }
                
                PacketBuffer.RemoveRange(0, (int)listStream.Position);
            }
            
            packet = CurrentPacket;
            CurrentPacket = null;

            return PacketBuffer.Count > 0;
        }

    }
}
