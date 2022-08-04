using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrueCraft.Core.World
{
    /// <summary>
    /// Represents a ray; a line with a start and direction, but no end.
    /// </summary>
    // Mostly taken from the MonoXna project, which is licensed under the MIT license
    public struct Ray : IEquatable<Ray>
    {
        #region Public Fields

        /// <summary>
        /// The direction and length of the ray.  This is NOT a unit vector.
        /// </summary>
        public readonly Vector3 Direction;

        /// <summary>
        /// The position of the ray (its origin).
        /// </summary>
        public readonly Vector3 Position;

        #endregion


        #region Public Constructors

        /// <summary>
        /// Creates a new ray from specified values.
        /// </summary>
        /// <param name="position">The position of the ray (its origin).</param>
        /// <param name="direction">The direction of the ray.</param>
        public Ray(Vector3 position, Vector3 direction)
        {
            this.Position = position;
            this.Direction = direction;
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Determines whether this and another object are equal.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            return (obj is Ray) && Equals((Ray)obj);
        }


        /// <summary>
        /// Determines whether this and another ray are equal.
        /// </summary>
        /// <param name="other">The other ray.</param>
        /// <returns></returns>
        public bool Equals(Ray other)
        {
            return Position.Equals(other.Position) && Direction.Equals(other.Direction);
        }

        /// <summary>
        /// Returns the hash code for this ray.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Position.GetHashCode() ^ Direction.GetHashCode();
        }

        /// <summary>
        /// Determines if this Ray intersects the Bounding Box.
        /// </summary>
        /// <param name="box">The Bounding Box to check for intersection.</param>
        /// <param name="distance">Returns the fraction of the Ray's length at which the
        /// intersection occurs.  If the Ray starts inside the box, this value will
        /// be clamped to zero.  If this method returns false, this is undefined.</param>
        /// <param name="face">Returns the BlockFace on which the Ray intersects the
        /// Bounding Box.  If this method returns false, this is undefined.</param>
        /// <returns>True if this Ray intersects the given Bounding Box; false otherwise.</returns>
        /// <remarks>
        /// <para>
        /// This is a modified version of the Ray-Box Intersection Algorithm in
        /// Reference A.  It was modified to determine which BlockFace the Ray
        /// intersects with.
        /// </para>
        /// </remarks>
        public bool Intersects(BoundingBox box, ref double distance, ref BlockFace face)
        {
            double txmin, txmax, tymin, tymax, tzmin, tzmax;

            double tmin = double.MaxValue;
            double tmax = double.MinValue;

            // t0 represents the start position of the Ray.
            double t0 = 0.0;
            // t1 represents the tip of the Ray.
            double t1 = Math.Sqrt(Direction.X * Direction.X + Direction.Y * Direction.Y + Direction.Z * Direction.Z);

            Vector3 normalizedDirection = Direction / t1;
            double invdx = 1.0 / normalizedDirection.X;
            double invdy = 1.0 / normalizedDirection.Y;
            double invdz = 1.0 / normalizedDirection.Z;

            if (invdx >= 0)
            {
                tmin = txmin = (box.Min.X - Position.X) * invdx;
                tmax = txmax = (box.Max.X - Position.X) * invdx;
            }
            else
            {
                tmin = txmin = (box.Max.X - Position.X) * invdx;
                tmax = txmax = (box.Min.X - Position.X) * invdx;
            }
            if (txmin > t1 || txmax < t0)
                return false;
            if (txmin > tmin) tmin = txmin;
            if (txmax < tmax) tmax = txmax;

            if (invdy >= 0)
            {
                tymin = (box.Min.Y - Position.Y) * invdy;
                tymax = (box.Max.Y - Position.Y) * invdy;
            }
            else
            {
                tymin = (box.Max.Y - Position.Y) * invdy;
                tymax = (box.Min.Y - Position.Y) * invdy;
            }
            if (tymin > tmax || tymax < tmin)
                return false;
            if (tymin > tmin) tmin = tymin;
            if (tymax < tmax) tmax = tymax;

            if (invdz >= 0)
            {
                tzmin = (box.Min.Z - Position.Z) * invdz;
                tzmax = (box.Max.Z - Position.Z) * invdz;
            }
            else
            {
                tzmin = (box.Max.Z - Position.Z) * invdz;
                tzmax = (box.Min.Z - Position.Z) * invdz;
            }
            if (tzmin > tmax || tzmax < tmin)
                return false;
            if (tzmin > tmin) tmin = tzmin;
            if (tzmax < tmax) tmax = tzmax;

            // If tmin > t1, the Ray starts past the Box.
            // If tmax < t0, the Ray ends prior to entering the Box.
            if (tmin > t1 || tmax < t0)
                return false;

            // If tmax == t0, either the Ray starts on the surface of the Box,
            // the Ray is very short, or the Box is very small.  (The case of
            // the Box being very small can be ignored.)
            // If tmax < t1, the Ray started on the Surface of the Box and terminated
            // outside the box.
            if (Math.Abs(tmax - t0) < GameConstants.Epsilon && tmax < t1)
            {   // T
                if (tmax == txmax)
                    face = invdx >= 0 ? BlockFace.PositiveX : BlockFace.NegativeX;
                else if (tmax == tymax)
                    face = invdy >= 0 ? BlockFace.PositiveY : BlockFace.NegativeY;
                else
                    face = invdz >= 0 ? BlockFace.PositiveZ : BlockFace.NegativeZ;
            }
            else
            {
                // As tmin was assigned from one of txmin, tymin or tzmin,
                // we can safely use exact equality checks.
                if (tmin == txmin)
                    face = invdx >= 0 ? BlockFace.NegativeX : BlockFace.PositiveX;
                else if (tmin == tymin)
                    face = invdy >= 0 ? BlockFace.NegativeY : BlockFace.PositiveY;
                else
                    face = invdz >= 0 ? BlockFace.NegativeZ : BlockFace.PositiveZ;
            }

            // Determine distance as fraction of the Ray's Length:
            distance = Math.Max(0, tmin / t1);

            return true;
        }

        public static bool operator !=(Ray a, Ray b)
        {
            return !a.Equals(b);
        }

        public static bool operator ==(Ray a, Ray b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Returns a string representation of this ray.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{{Position:{0} Direction:{1}}}", Position.ToString(), Direction.ToString());
        }

        #endregion
    }
}
