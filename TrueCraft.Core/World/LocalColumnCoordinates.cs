using System;

namespace TrueCraft.Core.World
{
    /// <summary>
    /// Local Column Coordinates specify the location of a Column of Blocks
    /// within a Chunk relative to the North-West Corner.
    /// </summary>
    public class LocalColumnCoordinates : IEquatable<LocalColumnCoordinates>
    {
        public LocalColumnCoordinates(int x, int z)
        {
            X = x;
            Z = z;
        }

        public int X { get; }
        public int Z { get; }

        /// <summary>
        /// Calculates the Euclidean distance between two Coordinates.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>The Euclidean distance to the other instance.</returns>
        public double DistanceTo(LocalColumnCoordinates other)
        {
            int dx = other.X - X;
            int dz = other.Z - Z;
            return Math.Sqrt(dx * dx + dz * dz);
        }

        #region IEquatable<> & related
        public bool Equals(LocalColumnCoordinates other)
        {
            if (object.ReferenceEquals(other, null))
                return false;
            return this.X == other.X && this.Z == other.Z;
        }

        public static bool operator==(LocalColumnCoordinates l, LocalColumnCoordinates r)
        {
            if (!object.ReferenceEquals(l, null))
            {
                if (!object.ReferenceEquals(r, null))
                    return l.Equals(r);
                else
                    return false;
            }
            else
            {
                if (!object.ReferenceEquals(r, null))
                    return false;
                else
                    return true;
            }
        }

        public static bool operator!=(LocalColumnCoordinates l, LocalColumnCoordinates r)
        {
            return !(l == r);
        }
        #endregion

        #region object overrides
        public override bool Equals(object obj)
        {
            return Equals(obj as LocalColumnCoordinates);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int rv = X * 17;
                rv += Z * 409;

                return rv;
            }
        }

        public override string ToString()
        {
            return $"<{X},{Z}>";
        }
        #endregion
    }
}
