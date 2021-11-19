using System;

namespace TrueCraft.Core.Networking.Packets
{
    public struct ChangeHeldItemPacket : IPacket
    {
        public byte ID { get { return 0x10; } }

        private short _slot;

        public ChangeHeldItemPacket(short slot)
        {
#if DEBUG
            if (slot < 0 || slot >= 9)    // NOTE: hard-coded constants for size of Hotbar!
                throw new ArgumentOutOfRangeException($"{slot} is not in the permissible hotbar range of [0,9).");
#endif

            _slot = slot;
        }

        public short Slot { get => _slot; }

        public void ReadPacket(IMinecraftStream stream)
        {
            _slot = stream.ReadInt16();
        }

        public void WritePacket(IMinecraftStream stream)
        {
            stream.WriteInt16(_slot);
        }
    }
}