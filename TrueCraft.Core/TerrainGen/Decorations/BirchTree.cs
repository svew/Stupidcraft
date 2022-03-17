using System;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.World;

namespace TrueCraft.Core.TerrainGen.Decorations
{
    public class BirchTree : Decoration
    {
        const int LeafRadius = 2;

        public override bool ValidLocation(LocalVoxelCoordinates location)
        {
            if (location.X - LeafRadius < 0
                || location.X + LeafRadius >= Chunk.Width
                || location.Z - LeafRadius < 0
                || location.Z + LeafRadius >= Chunk.Depth)
            {
                return false;
            }
            return true;
        }

        public override bool GenerateAt(int seed, IChunk chunk, LocalVoxelCoordinates location)
        {
            if (!ValidLocation(location))
                return false;

            Random random = new Random(seed);
            int height = random.Next(4, 5);
            GenerateColumn(chunk, location, height, WoodBlock.BlockID, 0x2);
            LocalVoxelCoordinates leafLocation = new LocalVoxelCoordinates(location.X, location.Y + height, location.Z);
            GenerateVanillaLeaves(chunk, leafLocation, LeafRadius, LeavesBlock.BlockID, 0x2);
            return true;
        }
    }
}