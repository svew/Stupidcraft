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
        /// Returns the distance along the ray where it intersects the specified bounding box, if it intersects at all.
        /// </summary>
        [Obsolete()]
        public double? Intersects(BoundingBox box, out BlockFace face)
        {
            face = BlockFace.PositiveY;
            //first test if start in box
            if (Position.X >= box.Min.X
                    && Position.X <= box.Max.X
                    && Position.Y >= box.Min.Y
                    && Position.Y <= box.Max.Y
                    && Position.Z >= box.Min.Z
                    && Position.Z <= box.Max.Z)
                return 0.0f;// here we concidere cube is full and origine is in cube so intersect at origine

            //Second we check each face
            Vector3 maxT = new Vector3(-1.0f);
            //Vector3 minT = new Vector3(-1.0f);
            //calcul intersection with each faces
            if (Direction.X != 0.0f)
            {
                if (Position.X < box.Min.X)
                    maxT.X = (box.Min.X - Position.X) / Direction.X;
                else if (Position.X > box.Max.X)
                    maxT.X = (box.Max.X - Position.X) / Direction.X;
            }

            if (Direction.Y != 0.0f)
            {
                if (Position.Y < box.Min.Y)
                    maxT.Y = (box.Min.Y - Position.Y) / Direction.Y;
                else if (Position.Y > box.Max.Y)
                    maxT.Y = (box.Max.Y - Position.Y) / Direction.Y;
            }

            if (Direction.Z != 0.0f)
            {
                if (Position.Z < box.Min.Z)
                    maxT.Z = (box.Min.Z - Position.Z) / Direction.Z;
                else if (Position.Z > box.Max.Z)
                    maxT.Z = (box.Max.Z - Position.Z) / Direction.Z;
            }

            //get the maximum maxT
            if (maxT.X > maxT.Y && maxT.X > maxT.Z)
            {
                if (maxT.X < 0.0f)
                    return null;// ray go on opposite of face
                //coordonate of hit point of face of cube
                double coord = Position.Z + maxT.X * Direction.Z;
                // if hit point coord ( intersect face with ray) is out of other plane coord it miss
                if (coord < box.Min.Z || coord > box.Max.Z)
                    return null;
                coord = Position.Y + maxT.X * Direction.Y;
                if (coord < box.Min.Y || coord > box.Max.Y)
                    return null;

                if (Position.X < box.Min.X)
                    face = BlockFace.NegativeX;
                else if (Position.X > box.Max.X)
                    face = BlockFace.PositiveX;

                return maxT.X;
            }
            if (maxT.Y > maxT.X && maxT.Y > maxT.Z)
            {
                if (maxT.Y < 0.0f)
                    return null;// ray go on opposite of face
                //coordonate of hit point of face of cube
                double coord = Position.Z + maxT.Y * Direction.Z;
                // if hit point coord ( intersect face with ray) is out of other plane coord it miss
                if (coord < box.Min.Z || coord > box.Max.Z)
                    return null;
                coord = Position.X + maxT.Y * Direction.X;
                if (coord < box.Min.X || coord > box.Max.X)
                    return null;

                if (Position.Y < box.Min.Y)
                    face = BlockFace.NegativeY;
                else if (Position.Y > box.Max.Y)
                    face = BlockFace.PositiveY;

                return maxT.Y;
            }
            else //Z
            {
                if (maxT.Z < 0.0f)
                    return null;// ray go on opposite of face
                //coordonate of hit point of face of cube
                double coord = Position.X + maxT.Z * Direction.X;
                // if hit point coord ( intersect face with ray) is out of other plane coord it miss
                if (coord < box.Min.X || coord > box.Max.X)
                    return null;
                coord = Position.Y + maxT.Z * Direction.Y;
                if (coord < box.Min.Y || coord > box.Max.Y)
                    return null;

                if (Position.Z < box.Min.Z)
                    face = BlockFace.NegativeZ;
                else if (Position.Z > box.Max.Z)
                    face = BlockFace.PositiveZ;

                return maxT.Z;
            }
        }

        /// <summary>
        /// Determines if this Ray intersects the Bounding Box.
        /// </summary>
        /// <param name="box">The Bounding Box to check for intersection.</param>
        /// <param name="distance">Returns the fraction of the Ray's length at which the
        /// intersection occurs.  If this method returns false, this is undefined.</param>
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
            double tmin, tmax, txmin, txmax, tymin, tymax, tzmin, tzmax;

            double t0 = 0.0;
            double t1 = Math.Sqrt(Direction.X * Direction.X + Direction.Y * Direction.Y + Direction.Z * Direction.Z);

            // if tmin were less than zero, it would be before the start of the Ray
            tmin = t0;
            // If tmax were greater than t1, it would be beyond the end of the Ray.
            tmax = t1;

            Vector3 normalizedDirection = Direction / t1;
            double invdx = 1.0 / normalizedDirection.X;
            double invdy = 1.0 / normalizedDirection.Y;
            double invdz = 1.0 / normalizedDirection.Z;

            if (invdx >= 0)
            {
                txmin = (box.Min.X - Position.X) * invdx;
                txmax = (box.Max.X - Position.X) * invdx;
            }
            else
            {
                txmin = (box.Max.X - Position.X) * invdx;
                txmax = (box.Min.X - Position.X) * invdx;
            }
            if (txmin > tmax || txmax < tmin)
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
            if (txmin > tmax || tymin > tmax)
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

            // Determine the BlockFace of the first intersection.
            if (Math.Abs(tmin) < GameConstants.Epsilon)
            {   // tmin is effectively zero.  Therefore, the Ray starts at the
                // surface of the Box.  It may be directed inwards or outwards.
                if (tmin == tmax)
                {   // The Ray is directed outwards from the surface of the Box.
                    if (tmin == txmax)
                        face = invdx >= 0 ? BlockFace.PositiveX : BlockFace.NegativeX;
                    else if (tmin == tymax)
                        face = invdy >= 0 ? BlockFace.PositiveY : BlockFace.NegativeY;
                    else
                        face = invdz >= 0 ? BlockFace.PositiveZ : BlockFace.NegativeZ;
                }
                else
                {   // The Ray is directed inwards from the surface of the Box.
                    if (tmin == txmin)
                        face = invdx >= 0 ? BlockFace.NegativeX : BlockFace.PositiveX;
                    else if (tmin == tymin)
                        face = invdy >= 0 ? BlockFace.NegativeY : BlockFace.PositiveY;
                    else
                        face = invdz >= 0 ? BlockFace.NegativeZ : BlockFace.PositiveZ;
                }
            }
            else if (tmin < 0)
            {   // The Ray starts within the Box.
                if (tmin == txmin)
                    face = invdx >= 0 ? BlockFace.PositiveX : BlockFace.NegativeX;
                else if (tmin == tymin)
                    face = invdy >= 0 ? BlockFace.PositiveY : BlockFace.NegativeY;
                else
                    face = invdz >= 0 ? BlockFace.PositiveZ : BlockFace.NegativeZ;
            }
            else  // tmin > 0
            {   // The ray starts prior to entering the Box.
                if (tmin == txmin)
                    face = invdx >= 0 ? BlockFace.NegativeX : BlockFace.PositiveX;
                else if (tmin == tymin)
                    face = invdy >= 0 ? BlockFace.NegativeY : BlockFace.PositiveY;
                else
                    face = invdz >= 0 ? BlockFace.NegativeZ : BlockFace.PositiveZ;
            }

            // Determine distance as fraction of the Ray's Length:
            distance = tmin / t1;

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
