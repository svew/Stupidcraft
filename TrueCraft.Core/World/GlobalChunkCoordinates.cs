using System;

namespace TrueCraft.Core.World
{
    /// <summary>
    /// Specifies the location of a Chunk in 2D Global Coordinates.  The count
    /// is in units of Chunks.
    /// </summary>
    /// <remarks>
    ///<para>
    /// These coordinates specify the number of Chunks away from the origin.
    ///</para>
    /// </remarks>
    public class GlobalChunkCoordinates : IEquatable<GlobalChunkCoordinates>
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
        public GlobalChunkCoordinates(int x, int z)
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
        public bool Equals(GlobalChunkCoordinates? other)
        {
            if (other is null)
                return false;
            else
                return this.X == other.X && this.Z == other.Z;
        }

        public static bool operator !=(GlobalChunkCoordinates? a, GlobalChunkCoordinates? b)
        {
            return !(a == b);
        }

        public static bool operator ==(GlobalChunkCoordinates? a, GlobalChunkCoordinates? b)
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
        #endregion // IEquatable<>

        #region object overrides
        /// <summary>
        /// Determines whether this and another object are equal.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as GlobalChunkCoordinates);
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
        /// Converts this GlobalChunkCoordinates to a string in the format &lt;x, z&gt;.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"<{X},{Z}>";
        }
        #endregion

        #region Conversion operators
        public static explicit operator GlobalChunkCoordinates(GlobalVoxelCoordinates value)
        {
            int chunkX;
            int chunkZ;

            if (value.X >= 0)
                chunkX = value.X / WorldConstants.ChunkWidth;
            else
                chunkX = (value.X + 1) / WorldConstants.ChunkWidth - 1;

            if (value.Z >= 0)
                chunkZ = (value.Z / WorldConstants.ChunkDepth);
            else
                chunkZ = (value.Z + 1) / WorldConstants.ChunkDepth - 1;

            return new GlobalChunkCoordinates(chunkX, chunkZ);
        }

        public static explicit operator GlobalChunkCoordinates(Vector3 value)
        {
            int x = (int)Math.Floor(value.X);
            int y = (int)Math.Floor(value.Y);
            int z = (int)Math.Floor(value.Z);
            int chunkX, chunkZ;

            if (x >= 0)
                chunkX = x / WorldConstants.ChunkWidth;
            else
                chunkX = (x + 1) / WorldConstants.ChunkWidth - 1;

            if (z >= 0)
                chunkZ = z / WorldConstants.ChunkDepth;
            else
                chunkZ = (z + 1) / WorldConstants.ChunkDepth - 1;

            return new GlobalChunkCoordinates(chunkX, chunkZ);
        }
        #endregion

        #region Constants
        public static readonly GlobalChunkCoordinates Zero = new GlobalChunkCoordinates(0, 0);
        #endregion
    }
}
