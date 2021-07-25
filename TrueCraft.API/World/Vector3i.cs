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

        #region constant vectors
        public static readonly Vector3i Up    = new Vector3i( 0,  1,  0);
        public static readonly Vector3i Down  = new Vector3i( 0, -1,  0);
        public static readonly Vector3i North = new Vector3i( 0,  0, -1);
        public static readonly Vector3i East  = new Vector3i( 1,  0,  0);
        public static readonly Vector3i South = new Vector3i( 0,  0,  1);
        public static readonly Vector3i West  = new Vector3i(-1,  0,  0);
        #endregion
    }
}
