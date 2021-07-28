using System;

namespace TrueCraft.API.World
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
        public bool Equals(LocalVoxelCoordinates other)
        {
            return other.X.Equals(X) && other.Y.Equals(Y) && other.Z.Equals(Z);
        }

        public static bool operator !=(LocalVoxelCoordinates a, LocalVoxelCoordinates b)
        {
            return !(a == b);
        }

        public static bool operator ==(LocalVoxelCoordinates a, LocalVoxelCoordinates b)
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
        #endregion  // IEquatable<LocalVoxelCoordinates>

        #region Object overrides
        /// <summary>
        /// Determines whether this and another object are equal.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
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
