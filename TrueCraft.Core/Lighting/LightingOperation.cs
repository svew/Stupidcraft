using System;

namespace TrueCraft.Core.Lighting
{
    public struct LightingOperation
    {
        private readonly BoundingBox _boundingBox;
        private readonly bool _skyLight;

        public LightingOperation(BoundingBox boundingBox, bool skyLight)
        {
            _boundingBox = boundingBox;
            _skyLight = skyLight;
        }

        public BoundingBox Box { get => _boundingBox; }

        public bool SkyLight { get => _skyLight; }
    }
}
