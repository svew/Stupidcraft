using System;

namespace TrueCraft.API.World
{
    /// <summary>
    /// Specifies the location of a Chunk within a Region
    /// </summary>
    /// <remarks>
    /// <para>
    /// These coordinates are relative to the North-West corner of the Region.
    /// The units of these Coordinates are Chunks.
    /// </para>
    /// </remarks>
    public class LocalChunkCoordinates : IEquatable<LocalChunkCoordinates>
    {
        /// <summary>
        /// The X component of the coordinates.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// The Z component of the coordinates.
        /// </summary>
        public int Z { get; }

        /// <summary>
        /// Creates a new trio of coordinates from the specified values.
        /// </summary>
        /// <param name="x">The X component of the coordinates.</param>
        /// <param name="z">The Z component of the coordinates.</param>
        public LocalChunkCoordinates(int x, int z)
        {
            X = x;
            Z = z;
        }

        #region IEquatable<LocalChunkCoordinates> & related

        /// <summary>
        /// Determines whether this 3D coordinates and another are equal.
        /// </summary>
        /// <param name="other">The other coordinates.</param>
        /// <returns></returns>
        public bool Equals(LocalChunkCoordinates other)
        {
            if (object.ReferenceEquals(other, null))
                return false;
            return this.X == other.X && this.Z == other.Z;
        }

        public static bool operator !=(LocalChunkCoordinates a, LocalChunkCoordinates b)
        {
            return !(a == b);
        }

        public static bool operator ==(LocalChunkCoordinates a, LocalChunkCoordinates b)
        {
            if (object.ReferenceEquals(a, null))
            {
                if (object.ReferenceEquals(b, null))
                    return true;
                else
                    return false;
            }
            else
            {
                if (object.ReferenceEquals(b, null))
                    return false;
                else
                    return a.Equals(b);

            }
        }
        #endregion  // IEquatable<LocalChunkCoordinates>

        #region Object overrides
        /// <summary>
        /// Determines whether this and another object are equal.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as LocalChunkCoordinates);
        }

        /// <summary>
        /// Returns the hash code for this 3D coordinates.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = X.GetHashCode();
                result = (result * 397) ^ Z.GetHashCode();
                return result;
            }
        }

        /// <summary>
        /// Converts this LocalChunkCoordinates to a string in the format &lt;x, y, z&gt;.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"<{X},{Z}>";
        }
        #endregion   // object overrides

        #region Conversion operators
        public static explicit operator LocalChunkCoordinates(GlobalChunkCoordinates value)
        {
            int localX;
            int localZ;

            if (value.X >= 0)
                localX = value.X / WorldConstants.RegionWidth;
            else
                localX = (value.X + 1) / WorldConstants.RegionWidth - 1;

            if (value.Z >= 0)
                localZ = value.Z / WorldConstants.RegionDepth;
            else
                localZ = (value.Z + 1) / WorldConstants.RegionDepth - 1;

            return new LocalChunkCoordinates(value.X - localX * WorldConstants.RegionWidth, value.Z - localZ * WorldConstants.RegionDepth);
        }
        #endregion
    }
}
