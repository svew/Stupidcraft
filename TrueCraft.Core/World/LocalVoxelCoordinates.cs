using System;

namespace TrueCraft.Core.World
{
    /// <summary>
    /// Specifies the location of a Voxel within a Chunk.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These coordinates specify the location of a Voxel within a chunk
    /// relative to the bottom-most Voxel in the North-West column.
    /// </para>
    /// </remarks>
    public class LocalVoxelCoordinates : IEquatable<LocalVoxelCoordinates>
    {
        /// <summary>
        /// The X component of the coordinates.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// The Y component of the coordinates.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// The Z component of the coordinates.
        /// </summary>
        public int Z { get; }

        /// <summary>
        /// Creates a new trio of coordinates from the specified values.
        /// </summary>
        /// <param name="x">The X component of the coordinates.</param>
        /// <param name="z">The Y component of the coordinates.</param>
        /// <param name="z">The Z component of the coordinates.</param>
        public LocalVoxelCoordinates(int x, int y, int z)
        {
#if DEBUG
            if (x < 0 || x >= WorldConstants.ChunkWidth)
                throw new ArgumentOutOfRangeException(nameof(x), x, $"{ nameof(x) } is outside the valid range of[0,{ WorldConstants.ChunkWidth - 1}]");
            if (z < 0 || z >= WorldConstants.ChunkDepth)
                throw new ArgumentOutOfRangeException(nameof(z), z, $"{ nameof(z) } is outside the valid range of[0,{ WorldConstants.ChunkDepth - 1}]");
#endif
            X = x;
            Y = y;
            Z = z;
        }

        #region IEquatable<LocalVoxelCoordinates> & related

        /// <summary>
        /// Determines whether this 3D coordinates and another are equal.
        /// </summary>
        /// <param name="other">The other coordinates.</param>
        /// <returns></returns>
        public bool Equals(LocalVoxelCoordinates? other)
        {
            if (other is null)
                return false;

            return other.X.Equals(X) && other.Y.Equals(Y) && other.Z.Equals(Z);
        }

        public static bool operator !=(LocalVoxelCoordinates? a, LocalVoxelCoordinates? b)
        {
            return !(a == b);
        }

        public static bool operator ==(LocalVoxelCoordinates? a, LocalVoxelCoordinates? b)
        {
            if (a is null)
            {
                if (b is null)
                    return true;
                else
                    return false;
            }
            else
            {
                if (b is null)
                    return false;
                else
                    return a.Equals(b);

            }
        }
        #endregion  // IEquatable<LocalVoxelCoordinates>

        #region Object overrides
        /// <summary>
        /// Determines whether this and another object are equal.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as LocalVoxelCoordinates);
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
                result = (result * 397) ^ Y.GetHashCode();
                result = (result * 397) ^ Z.GetHashCode();
                return result;
            }
        }

        /// <summary>
        /// Converts this LocalVoxelCoordinates to a string in the format &lt;x, y, z&gt;.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("<{0},{1},{2}>", X, Y, Z);
        }
        #endregion   // object overrides

        #region operators

        public LocalVoxelCoordinates Add(Vector3i other)
        {
            return new LocalVoxelCoordinates(this.X + other.X, this.Y + other.Y, this.Z + other.Z);
        }

        public static LocalVoxelCoordinates operator +(LocalVoxelCoordinates c, Vector3i v)
        {
            return c.Add(v);
        }

        public static LocalVoxelCoordinates operator +(Vector3i v, LocalVoxelCoordinates c)
        {
            return c.Add(v);
        }
        #endregion

        #region conversion operators
        public static explicit operator LocalVoxelCoordinates(GlobalVoxelCoordinates value)
        {
            int localX, localZ;

            if (value.X >= 0)
                localX = value.X % WorldConstants.ChunkWidth;
            else
                localX = WorldConstants.ChunkWidth - 1 - (-value.X - 1) % WorldConstants.ChunkWidth;

            if (value.Z >= 0)
                localZ = value.Z % WorldConstants.ChunkDepth;
            else
                localZ = WorldConstants.ChunkDepth - 1 - (-value.Z - 1) % WorldConstants.ChunkDepth;

            return new LocalVoxelCoordinates(localX, value.Y, localZ);

        }
        #endregion

        #region Constants
        public static readonly LocalVoxelCoordinates Zero = new LocalVoxelCoordinates(0, 0, 0);
        #endregion
    }
}
