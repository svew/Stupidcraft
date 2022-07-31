using System;
using TrueCraft.Core;
using TrueCraft.Core.Logic;
using TrueCraft.Core.World;

namespace TrueCraft.Client
{
    /// <summary>
    /// Efficient ray caster that can cast a ray into a voxel map
    /// and return the voxel that it intersects with.
    /// </summary>
    public static class VoxelCast
    {
        // Thanks to http://gamedev.stackexchange.com/questions/47362/cast-ray-to-select-block-in-voxel-game

        public static Tuple<GlobalVoxelCoordinates, BlockFace>? Cast(IDimension dimension,
            Ray ray, IBlockRepository repository, int posmax, int negmax)
        {
            // TODO: There are more efficient ways of doing this, fwiw

            BlockFace _face = BlockFace.PositiveY;
            double min = negmax * 2;
            GlobalVoxelCoordinates? pick = null;
            var face = BlockFace.PositiveY;
            for (int x = -posmax; x <= posmax; x++)
            {
                for (int y = -negmax; y <= posmax; y++)
                {
                    for (int z = -posmax; z <= posmax; z++)
                    {
                        GlobalVoxelCoordinates coords = (GlobalVoxelCoordinates)(new Vector3(x, y, z) + ray.Position).Round();
                        if (!dimension.IsValidPosition(coords))
                            continue;
                        var id = dimension.GetBlockID(coords);
                        if (id != 0)
                        {
                            var provider = repository.GetBlockProvider(id);
                            var box = provider.InteractiveBoundingBox;
                            if (box != null)
                            {
                                double distance = double.MaxValue;
                                if (ray.Intersects(box.Value.OffsetBy((Vector3)coords), ref distance, ref _face) && distance < min)
                                {
                                    min = distance;
                                    pick = coords;
                                    face = _face;
                                }
                            }
                        }
                    }
                }
            }

            if (pick is null)
                return null;

            return new Tuple<GlobalVoxelCoordinates, BlockFace>(pick, face);
        }
    }
}
