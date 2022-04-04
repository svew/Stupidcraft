using System;
using System.Diagnostics;

namespace TrueCraft.Core.Lighting
{
    /// <summary>
    /// A 3-dimensional array of boolean values.
    /// </summary>
    public class Bool3D
    {
        private readonly int _xsize;
        private readonly int _ysize;
        private readonly int _zsize;
        private readonly ulong[] _array;

        /// <summary>
        /// Constructs a new Bool3D.
        /// </summary>
        /// <param name="xsize">The size of the array in the x-dimension.</param>
        /// <param name="ysize">The size of the array in the y-dimension.</param>
        /// <param name="zsize">The size of the array in the z-dimension.</param>
        /// <param name="initial">The initial value of all entries in the array.</param>
        public Bool3D(int xsize, int ysize, int zsize, bool initial)
        {
            _xsize = xsize;
            _ysize = ysize;
            _zsize = zsize;

            int len = (xsize * ysize * zsize + 63) / 64;
            _array = new ulong[len];
            if (initial)
                for (int j = 0; j < len; j++)
                    _array[j] = ulong.MaxValue;
        }

        /// <summary>
        /// Gets or sets the boolean value at the given indices.
        /// </summary>
        /// <param name="x">The x-index of the boolean to retrieve.</param>
        /// <param name="y">The y-index of the boolean to retrieve.</param>
        /// <param name="z">The z-index of the boolean to retrieve.</param>
        /// <returns>The value of the specified boolean.</returns>
        public bool this[int x, int y, int z]
        {
            get
            {
                (ulong bitval, int longIndex) = Index(x, y, z);
                return (_array[longIndex] & bitval) != 0;
            }
            set
            {
                (ulong bitval, int longIndex) = Index(x, y, z);
                if (value)
                    _array[longIndex] |= bitval;
                else
                    _array[longIndex] &= ~bitval;
            }
        }

        /// <summary>
        /// Converts the given three indices into the indices into the internal
        /// 1-dimensional array.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>A tuple with first item set to the numeric value of the bit within a
        /// ulong, and the second item set to the index into the internal
        /// 1-dimensional array.</returns>
        private (ulong bitIndex, int arrayIndex) Index(int x, int y, int z)
        {
            ValidateIndex(x, _xsize, nameof(x));
            ValidateIndex(y, _ysize, nameof(y));
            ValidateIndex(z, _zsize, nameof(z));
            int index = (x * _zsize + z) * _ysize + y;
            int bitIndex = index & 0x003f;
            int arrayIndex = index >> 6;
            return (1ul << bitIndex, arrayIndex);
        }

        /// <summary>
        /// Validates that the given index is greater than or equal to zero and
        /// less than the given maximum.
        /// </summary>
        /// <param name="idx">The index to validate.</param>
        /// <param name="max">The maximum (non-inclusive) permissible value for the index.</param>
        /// <param name="name">The name of the index being validated.</param>
        [Conditional("DEBUG")]
        private static void ValidateIndex(int idx, int max, string name)
        {
            if (idx < 0 || idx > max)
                throw new IndexOutOfRangeException($"{name} = {idx} is outside the range of [0, {max})");
        }
    }
}
