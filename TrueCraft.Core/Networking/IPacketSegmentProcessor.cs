using System;
using System.Diagnostics.CodeAnalysis;

namespace TrueCraft.Core.Networking
{
    public interface IPacketSegmentProcessor
    {
        bool ProcessNextSegment(byte[] nextSegment, int offset, int len, [MaybeNullWhen(false)] out IPacket packet);
    }
}
