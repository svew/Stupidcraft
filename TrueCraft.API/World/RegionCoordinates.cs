using System;

namespace TrueCraft.API.World
{
    /// <summary>
    /// Specifies the location of a vertical column of Voxels in 2D Global Coordinates.
    /// </summary>
    /// <remarks>
    ///<para>
    /// These coordinates specify the number of Blocks/Voxels away from the origin.
    ///</para>
    /// </remarks>
    public class RegionCoordinates : IEquatable<RegionCoordinates>
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
        public RegionCoordinates(int x, int z)
        {
            X = x;
            Z = z;
        }

        #region IEquatable<> & related

        /// <summary>
        /// Determines whether this 3D coordinates and another are equal.
        /// </summary>
        /// <param name="other">The other coordinates.</param>
        /// <returns></returns>
        public bool Equals(RegionCoordinates other)
        {
            if (object.ReferenceEquals(other, null))
                return false;
            else
                return this.X == other.X && this.Z == other.Z;
        }

        public static bool operator !=(RegionCoordinates a, RegionCoordinates b)
        {
            return !(a == b);
        }

        public static bool operator ==(RegionCoordinates a, RegionCoordinates b)
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
        #endregion // IEquatable<>

        #region object overrides
        /// <summary>
        /// Determines whether this and another object are equal.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as RegionCoordinates);
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
        /// Converts this RegionCoordinates to a string in the format &lt;x, z&gt;.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"<{X},{Z}>";
        }
        #endregion // object overrides

        #region Conversions
        public static explicit operator RegionCoordinates(GlobalChunkCoordinates value)
        {
            int regionX;
            int regionZ;

            if (value.X >= 0)
                regionX = value.X / WorldConstants.RegionWidth;
            else
                regionX = (value.X + 1) / WorldConstants.RegionWidth - 1;

            if (value.Z >= 0)
                regionZ = value.Z / WorldConstants.RegionDepth;
            else
                regionZ = (value.Z + 1) / WorldConstants.RegionDepth - 1;

            return new RegionCoordinates(regionX, regionZ);
        }

        public static explicit operator RegionCoordinates(GlobalColumnCoordinates value)
        {
            return Convert(value.X, value.Z);
        }

        public static explicit operator RegionCoordinates(GlobalVoxelCoordinates value)
        {
            return Convert(value.X, value.Z);
        }

        private static RegionCoordinates Convert(int x, int z)
        {
            int regionX;
            int regionZ;

            if (x >= 0)
                regionX = x / (WorldConstants.ChunkWidth * WorldConstants.RegionWidth);
            else
                regionX = (x + 1) / (WorldConstants.ChunkWidth * WorldConstants.RegionWidth) - 1;

            if (z >= 0)
                regionZ = z / (WorldConstants.ChunkDepth * WorldConstants.RegionDepth);
            else
                regionZ = (z + 1) / (WorldConstants.ChunkDepth * WorldConstants.RegionDepth) - 1;

            return new RegionCoordinates(regionX, regionZ);
        }

        public GlobalChunkCoordinates GetGlobalChunkCoordinates(LocalChunkCoordinates value)
        {
            int x = this.X * WorldConstants.RegionWidth + value.X;
            int z = this.Z * WorldConstants.RegionDepth + value.Z;

            return new GlobalChunkCoordinates(x, z);
        }
        #endregion

        #region Constants
        public static readonly RegionCoordinates Zero = new RegionCoordinates(0, 0);
        #endregion
    }
}
