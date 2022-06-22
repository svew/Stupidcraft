using System;

namespace TrueCraft.Core.World
{
    /// <summary>
    /// Specifies the location of a vertical column of Voxels in 2D Global Coordinates.
    /// </summary>
    /// <remarks>
    ///<para>
    /// These coordinates specify the number of Blocks/Voxels away from the origin.
    ///</para>
    /// </remarks>
    public class GlobalColumnCoordinates : IEquatable<GlobalColumnCoordinates>
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
        public GlobalColumnCoordinates(int x, int z)
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
        public bool Equals(GlobalColumnCoordinates? other)
        {
            if (other is null)
                return false;
            else
                return this.X == other.X && this.Z == other.Z;
        }

        public static bool operator !=(GlobalColumnCoordinates? a, GlobalColumnCoordinates? b)
        {
            return !(a == b);
        }

        public static bool operator ==(GlobalColumnCoordinates? a, GlobalColumnCoordinates? b)
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
        public override bool Equals(object? obj)
        {
            return Equals(obj as GlobalColumnCoordinates);
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
        /// Converts this GlobalColumnCoordinates to a string in the format &lt;x, z&gt;.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"<{X},{Z}>";
        }
        #endregion
    }
}
