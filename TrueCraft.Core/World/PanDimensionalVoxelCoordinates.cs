using System;
namespace TrueCraft.Core.World
{
    public class PanDimensionalVoxelCoordinates : IEquatable<PanDimensionalVoxelCoordinates>
    {
        private readonly DimensionID _dimensionID;
        private readonly int _x;
        private readonly int _y;
        private readonly int _z;

        public PanDimensionalVoxelCoordinates(DimensionID dimensionID, int x, int y, int z)
        {
            _dimensionID = dimensionID;
            _x = x;
            _y = y;
            _z = z;
        }

        public DimensionID DimensionID { get => _dimensionID; }
        public int X { get => _x; }
        public int Y { get => _y; }
        public int Z { get => _z; }

        #region object overrides
        public override bool Equals(object obj)
        {
            return Equals(obj as PanDimensionalVoxelCoordinates);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int rv = (int)_dimensionID;
                rv = (rv * 397) ^ X.GetHashCode();
                rv = (rv * 397) ^ Y.GetHashCode();
                rv = (rv * 397) ^ Z.GetHashCode();
                return rv;
            }
        }
        #endregion

        #region IEquatable<PanDimensionVoxelCoordinates> & related
        public bool Equals(PanDimensionalVoxelCoordinates other)
        {
            if (other is null)
                return false;

            return _dimensionID == other.DimensionID &&
                _x == other.X && _y == other.Y && _z == other.Z;
        }

        public static bool operator==(PanDimensionalVoxelCoordinates l, PanDimensionalVoxelCoordinates r)
        {
            if (l is null)
            {
                if (r is null)
                    return true;
                else
                    return false;
            }
            else
            {
                return l.Equals(r);
            }
        }

        public static bool operator!=(PanDimensionalVoxelCoordinates l, PanDimensionalVoxelCoordinates r)
        {
            return !(l == r);
        }
        #endregion
    }
}
