using System;
using System.Collections;
using System.Collections.Generic;
using TrueCraft.Core.World;

namespace TrueCraft.Core
{
    public class PathResult : IList<GlobalVoxelCoordinates>
    {
        private int _index;

        private readonly IList<GlobalVoxelCoordinates> _wayPoints;

        /// <summary>
        /// Constructs a new PathResult
        /// </summary>
        /// <param name="wayPoints">The list of Points in the PathResult.  For performance reasons,
        /// the reference is simply copied.</param>
        public PathResult(IList<GlobalVoxelCoordinates> wayPoints)
        {
            _index = 0;
            _wayPoints = wayPoints;
        }

        /// <summary>
        /// A convenience for the User of the PathResult to track its progress
        /// within the PathResult.
        /// </summary>
        public int Index
        {
            get => _index;
            set
            {
                if (value < 0 || value >= _wayPoints.Count)
                    throw new ArgumentOutOfRangeException();

                _index = value;
            }
        }

        /// <inheritdoc />
        public int Count { get => _wayPoints.Count; }

        /// <inheritdoc />
        public bool IsReadOnly { get => true; }

        /// <inheritdoc />
        public GlobalVoxelCoordinates this[int index]
        {
            get => _wayPoints[index];
            set => throw new NotSupportedException();
        }

        public int IndexOf(GlobalVoxelCoordinates item)
        {
            return _wayPoints.IndexOf(item);
        }

        public void Insert(int index, GlobalVoxelCoordinates item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public void Add(GlobalVoxelCoordinates item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(GlobalVoxelCoordinates item)
        {
            return _wayPoints.Contains(item);
        }

        public void CopyTo(GlobalVoxelCoordinates[] array, int arrayIndex)
        {
            _wayPoints.CopyTo(array, arrayIndex);
        }

        public bool Remove(GlobalVoxelCoordinates item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<GlobalVoxelCoordinates> GetEnumerator()
        {
            return _wayPoints.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _wayPoints.GetEnumerator();
        }
    }
}
