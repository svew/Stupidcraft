using System;
using TrueCraft.API.Networking;
using TrueCraft.API.Windows;

namespace TrueCraft.Core.Networking.Packets
{
    /// <summary>
    /// Instructs the client to open an inventory window.
    /// </summary>
    public struct OpenWindowPacket : IPacket
    {
        public byte ID { get { return 0x64; } }

        public OpenWindowPacket(sbyte windowID, WindowType type, string title, sbyte totalSlots)
        {
            WindowID = windowID;
            Type = type;
            Title = title;
            TotalSlots = totalSlots;
        }

        public sbyte WindowID { get; private set; }

        public WindowType Type { get; private set; }

        public string Title { get; private set; }

        public sbyte TotalSlots { get; private set; }

        public void ReadPacket(IMinecraftStream stream)
        {
            WindowID = stream.ReadInt8();
            Type = (WindowType)stream.ReadInt8();
            Title = stream.ReadString8();
            TotalSlots = stream.ReadInt8();
        }

        public void WritePacket(IMinecraftStream stream)
        {
            stream.WriteInt8(WindowID);
            stream.WriteInt8((sbyte)Type);
            stream.WriteString8(Title);
            stream.WriteInt8(TotalSlots);
        }
    }
}