using System;
using System.Linq;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.World;

namespace TrueCraft.Core.TerrainGen.Decorators
{
    class FreezeDecorator : IChunkDecorator
    {
        public void Decorate(IDimension dimension, IChunk chunk, IBiomeRepository biomes)
        {
            for (int x = 0; x < Chunk.Width; x++)
            {
                for (int z = 0; z < Chunk.Depth; z++)
                {
                    IBiomeProvider biome = biomes.GetBiome(chunk.GetBiome(x, z));
                    if (biome.Temperature < 0.15)
                    {
                        int height = chunk.GetHeight(x, z);
                        for (int y = height; y < Chunk.Height; y++)
                        {
                            var location = new LocalVoxelCoordinates(x, y, z);
                            if (chunk.GetBlockID(location).Equals(StationaryWaterBlock.BlockID) || chunk.GetBlockID(location).Equals(WaterBlock.BlockID))
                                chunk.SetBlockID(location, IceBlock.BlockID);
                            else
                            {
                                var below = chunk.GetBlockID(location);
                                byte[] whitelist =
                                {
                                    DirtBlock.BlockID,
                                    GrassBlock.BlockID,
                                    IceBlock.BlockID,
                                    LeavesBlock.BlockID
                                };
                                if (y == height && whitelist.Any(w => w == below))
                                {
                                    if (chunk.GetBlockID(location).Equals(IceBlock.BlockID) && CoverIce(chunk, biomes, location))
                                        chunk.SetBlockID(new LocalVoxelCoordinates(location.X, location.Y + 1, location.Z), SnowfallBlock.BlockID);
                                    else if (!chunk.GetBlockID(location).Equals(SnowfallBlock.BlockID) && !chunk.GetBlockID(location).Equals(AirBlock.BlockID))
                                        chunk.SetBlockID(new LocalVoxelCoordinates(location.X, location.Y + 1, location.Z), SnowfallBlock.BlockID);
                                }
                            }
                        }
                    }
                }
            }
        }

        bool CoverIce(IChunk chunk, IBiomeRepository biomes, LocalVoxelCoordinates location)
        {
            const int maxDistance = 4;
            Vector3i[] nearby = new Vector3i[]
            {
                maxDistance * Vector3i.West,
                maxDistance * Vector3i.East,
                maxDistance * Vector3i.South,
                maxDistance * Vector3i.North
            };
            for (int i = 0; i < nearby.Length; i++)
            {
                int checkX = location.X + nearby[i].X;
                int checkZ = location.Z + nearby[i].Z;
                // TODO: does the order of the nearby array produce peculiar direction-dependent variations
                //       in snow cover near chunk boundaries?
                if (checkX < 0 || checkX >= Chunk.Width || checkZ < 0 || checkZ >= Chunk.Depth)
                    return false;
                LocalVoxelCoordinates check = new LocalVoxelCoordinates(checkX, location.Y, checkZ);
                IBiomeProvider biome = biomes.GetBiome(chunk.GetBiome(checkX, checkZ));
                if (chunk.GetBlockID(check).Equals(biome.SurfaceBlock) || chunk.GetBlockID(check).Equals(biome.FillerBlock))
                    return true;
            }
            return false;
        }
    }
}