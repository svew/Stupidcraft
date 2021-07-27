using System;

namespace TrueCraft.API.World
{
    /// <summary>
    /// Specifies the location of a Voxel in 3D Global Coordinates.
    /// </summary>
    /// <remarks>
    ///<para>
    /// These coordinates specify the number of Blocks/Voxels away from the origin.
    ///</para>
    /// </remarks>
    public class GlobalVoxelCoordinates : IEquatable<GlobalVoxelCoordinates>
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
        public GlobalVoxelCoordinates(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Creates a new instance of GlobalVoxelCoordinates that is a copy of the given one.
        /// </summary>
        /// <param name="other"></param>
        public GlobalVoxelCoordinates(GlobalVoxelCoordinates other)
        {
            X = other.X;
            Y = other.Y;
            Z = other.Z;
        }

        /// <summary>
        /// Converts this GlobalVoxelCoordinates to a string in the format &lt;x, y, z&gt;.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"<{X},{Y},{Z}>";
        }

        #region Math

        ///// <summary>
        ///// Clamps the coordinates to within the specified value.
        ///// </summary>
        ///// <param name="value">Value.</param>
        //public void Clamp(int value)
        //{
        //    // TODO: Fix for negative values
        //    if (Math.Abs(X) > value)
        //        X = value * (X < 0 ? -1 : 1);
        //    if (Math.Abs(Y) > value)
        //        Y = value * (Y < 0 ? -1 : 1);
        //    if (Math.Abs(Z) > value)
        //        Z = value * (Z < 0 ? -1 : 1);
        //}

        /// <summary>
        /// Calculates the distance between two GlobalVoxelCoordinates objects.
        /// </summary>
        public double DistanceTo(GlobalVoxelCoordinates other)
        {
            return Math.Sqrt(Square(other.X - X) +
                             Square(other.Y - Y) +
                             Square(other.Z - Z));
        }

        /// <summary>
        /// Calculates the square of a num.
        /// </summary>
        private int Square(int num)
        {
            return num * num;
        }

        /// <summary>
        /// Finds the distance of this Coordinate3D from GlobalVoxelCoordinates.Zero
        /// </summary>
        public double Distance
        {
            get
            {
                return DistanceTo(Zero);
            }
        }

        /// <summary>
        /// Returns the component-wise minimum of two 3D coordinates.
        /// </summary>
        /// <param name="value1">The first coordinates.</param>
        /// <param name="value2">The second coordinates.</param>
        /// <returns></returns>
        public static GlobalVoxelCoordinates Min(GlobalVoxelCoordinates value1, GlobalVoxelCoordinates value2)
        {
            return new GlobalVoxelCoordinates(
                Math.Min(value1.X, value2.X),
                Math.Min(value1.Y, value2.Y),
                Math.Min(value1.Z, value2.Z)
                );
        }

        /// <summary>
        /// Returns the component-wise maximum of two 3D coordinates.
        /// </summary>
        /// <param name="value1">The first coordinates.</param>
        /// <param name="value2">The second coordinates.</param>
        /// <returns></returns>
        public static GlobalVoxelCoordinates Max(GlobalVoxelCoordinates value1, GlobalVoxelCoordinates value2)
        {
            return new GlobalVoxelCoordinates(
                Math.Max(value1.X, value2.X),
                Math.Max(value1.Y, value2.Y),
                Math.Max(value1.Z, value2.Z)
                );
        }

        #endregion

        #region Operators

        public static bool operator !=(GlobalVoxelCoordinates a, GlobalVoxelCoordinates b)
        {
            return !(a == b);
        }

        public static bool operator ==(GlobalVoxelCoordinates a, GlobalVoxelCoordinates b)
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

        public GlobalVoxelCoordinates Add(Vector3i other)
        {
            return new GlobalVoxelCoordinates(this.X + other.X, this.Y + other.Y, this.Z + other.Z);
        }

        public static GlobalVoxelCoordinates operator+(GlobalVoxelCoordinates c, Vector3i v)
        {
            return c.Add(v);
        }

        public static GlobalVoxelCoordinates operator +(Vector3i v, GlobalVoxelCoordinates c)
        {
            return c.Add(v);
        }

        public static Vector3i operator-(GlobalVoxelCoordinates l, GlobalVoxelCoordinates r)
        {
            return new Vector3i(l.X - r.X, l.Y - r.Y, l.Z - r.Z);
        }
        #endregion

        #region Conversion operators

        public static explicit operator GlobalVoxelCoordinates(GlobalColumnCoordinates a)
        {
            return new GlobalVoxelCoordinates(a.X, 0, a.Z);
        }

        public static explicit operator GlobalVoxelCoordinates(Vector3 a)
        {
            return new GlobalVoxelCoordinates((int)a.X,
                                     (int)a.Y,
                                     (int)a.Z);
        }

        /// <summary>
        /// Converts Global Chunk Coordinates to Global Voxel Coordinates
        /// </summary>
        /// <param name="a">The Global Chunk Coordinates to convert.</param>
        /// <returns>
        /// The Global Voxel Coordinates of the bottom Block of the North-West column of
        /// Blocks in the specified Chunk.
        /// </returns>
        public static explicit operator GlobalVoxelCoordinates(GlobalChunkCoordinates a)
        {
            return new GlobalVoxelCoordinates(WorldConstants.ChunkWidth * a.X, 0, WorldConstants.ChunkDepth * a.Z);
        }
        #endregion

        #region Constants

        /// <summary>
        /// A trio of 3D coordinates with components set to 0.0.
        /// </summary>
        public static readonly GlobalVoxelCoordinates Zero = new GlobalVoxelCoordinates(0, 0, 0);
        #endregion

        /// <summary>
        /// Determines whether this 3D coordinates and another are equal.
        /// </summary>
        /// <param name="other">The other coordinates.</param>
        /// <returns></returns>
        public bool Equals(GlobalVoxelCoordinates other)
        {
            if (object.ReferenceEquals(other, null))
                return false;
            else
                return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }

        /// <summary>
        /// Determines whether this and another object are equal.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as GlobalVoxelCoordinates);
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
    }
}
