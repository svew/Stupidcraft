using fNbt;
using fNbt.Serialization;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace TrueCraft.Core
{
    /// <summary>
    /// Represents a slice of an array of 4-bit values.
    /// </summary>
    public class NybbleArray : INbtSerializable
    {
        /// <summary>
        /// The data in the nibble array. Each byte contains
        /// two nybbles.  The low order nybble corresponds to the
        /// lower numbered index.
        /// </summary>
        private byte[] _data;

        /// <summary>
        /// Constructs a new NybbleArray
        /// </summary>
        /// <remarks>Objects constructed with this constructor are intended for
        /// use in deserializing NBT data.</remarks>
        public NybbleArray()
        {
            _data = new byte[0];
        }

        /// <summary>
        ///  Constructs a new NybbleArray by copying data from the given array.
        /// </summary>
        /// <param name="data">The source array.</param>
        /// <param name="offset">The offset (in bytes) within the source array of
        /// the first element of the new NybbleArray.</param>
        /// <param name="length">The length (in nybbles) of the data.  The number of
        /// bytes required in the source array is half this value.</param>
        public NybbleArray(byte[] data, int offset, int length)
        {
            length /= 2;
            _data = new byte[length];
            Buffer.BlockCopy(data, offset, _data, 0, length);
        }

        /// <summary>
        ///  Constructs a new NybbleArray by copying data from the given Stream.
        /// </summary>
        /// <param name="stream">The Stream from which to read the data.</param>
        /// <param name="length">The length in Nybbles of the data to read.</param>
        public NybbleArray(Stream stream, int length)
        {
            length /= 2;
            _data = new byte[length];
            stream.Read(_data, 0, length);
        }

        /// <summary>
        /// Gets or sets a nibble at the given index.
        /// </summary>
        [NbtIgnore]
        public byte this[int index]
        {
            get
            {
                if (index < 0)
                    throw new IndexOutOfRangeException();
                return (byte)(_data[index / 2] >> (index % 2 * 4) & 0xF);
            }
            set
            {
                if (index < 0)
                    throw new IndexOutOfRangeException();
                value &= 0x0F;
                int idx = index / 2;
                if ((index & 0x01) != 0)
                    _data[idx] = (byte)((_data[idx] & 0x0F) | (value << 4));
                else
                    _data[idx] = (byte)((_data[idx] & 0xF0) | value);
            }
        }

        /// <summary>
        /// Gets the Length (in Nybbles) of this Array.
        /// </summary>
        public int Length { get => 2 * _data.Length; }

        public byte[] ToArray()
        {
            byte[] array = new byte[_data.Length];
            Buffer.BlockCopy(_data, 0, array, 0, _data.Length);
            return array;
        }

        public NbtTag Serialize(string tagName)
        {
            return new NbtByteArray(tagName, ToArray());
        }

        public void Deserialize(NbtTag value)
        {
            _data = new byte[value.ByteArrayValue.Length];
            Buffer.BlockCopy(value.ByteArrayValue, 0, _data, 0, _data.Length);
        }
    }
}
