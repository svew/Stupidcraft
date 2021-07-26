using System;

namespace TrueCraft.API.World
{
    public class Vector3i : IEquatable<Vector3i>
    {
        public Vector3i(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public static Vector3i operator+(Vector3i l, Vector3i r)
        {
            return new Vector3i(l.X + r.X, l.Y + r.Y, l.Z + r.Z);
        }

        /// <summary>
        /// Clamps the coordinates to within the specified value.
        /// </summary>
        /// <params>
        /// <param name="value">Value.</param>
        /// </params>
        public Vector3i Clamp(int value)
        {
            int x, y, z;

#if DEBUG
            if (value < 0) throw new ArgumentOutOfRangeException($"{nameof(value)} must be non - negative.");
#endif

            if (Math.Abs(X) > value)
                x = value * (X < 0 ? -1 : 1);
            else
                x = X;
            if (Math.Abs(Y) > value)
                y = value * (Y < 0 ? -1 : 1);
            else
                y = Y;
            if (Math.Abs(Z) > value)
                z = value * (Z < 0 ? -1 : 1);
            else
                z = Z;

            return new Vector3i(x, y, z);
        }


        #region Object overrides
        public override bool Equals(object obj)
        {
            return Equals(obj as Vector3i);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int rv = X * 409;
                rv *= 409 * Y;
                rv *= 397 * Z;

                return rv;
            }
        }

        public override string ToString()
        {
            return $"<{X},{Y},{Z}>";
        }
        #endregion

        #region IEquatable<> & related
        public bool Equals(Vector3i other)
        {
            if (object.ReferenceEquals(other, null))
                return false;

            return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }

        public static bool operator ==(Vector3i l, Vector3i r)
        {
            if (object.ReferenceEquals(l, null))
            {
                if (object.ReferenceEquals(r, null))
                    return true;
                else
                    return false;
            }
            else
            {
                if (object.ReferenceEquals(r, null))
                    return false;
                else
                    return l.Equals(r);
            }
        }

        public static bool operator !=(Vector3i l , Vector3i r)
        {
            return !(l == r);
        }
        #endregion

        public static Vector3i operator-(Vector3i arg)
        {
            return new Vector3i(-arg.X, -arg.Y, -arg.Z);
        }

        public static Vector3i operator*(Vector3i v, int a)
        {
            return new Vector3i(a * v.X, a * v.Y, a * v.Z);
        }

        public static Vector3i operator*(int a, Vector3i v)
        {
            return new Vector3i(a * v.X, a * v.Y, a * v.Z);
        }

        #region constant vectors
        public static readonly Vector3i Zero  = new Vector3i( 0,  0,  0);
        public static readonly Vector3i One   = new Vector3i( 1,  1,  1);
        public static readonly Vector3i Up    = new Vector3i( 0,  1,  0);
        public static readonly Vector3i Down  = new Vector3i( 0, -1,  0);
        public static readonly Vector3i North = new Vector3i( 0,  0, -1);
        public static readonly Vector3i East  = new Vector3i( 1,  0,  0);
        public static readonly Vector3i South = new Vector3i( 0,  0,  1);
        public static readonly Vector3i West  = new Vector3i(-1,  0,  0);

        /// <summary>
        /// A set of Vector3i objects pointing to the 4-connected neighbors at the
        /// same Y-level.
        /// </summary>
        public static readonly Vector3i[] Neighbors4 = { North, East, South, West };
        #endregion
    }
}
