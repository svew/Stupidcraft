using System;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.World;

namespace TrueCraft.Core.TerrainGen.Decorators
{
    public class LiquidDecorator : IChunkDecorator
    {
        public static readonly int WaterLevel = 40;

        public void Decorate(int seed, IChunk chunk, IBlockRepository _, IBiomeRepository biomes)
        {
            for (int x = 0; x < Chunk.Width; x++)
            {
                for (int z = 0; z < Chunk.Depth; z++)
                {
                    IBiomeProvider biome = biomes.GetBiome(chunk.GetBiome(x, z));
                    int height = chunk.GetHeight(x, z);
                    for (int y = height; y <= WaterLevel; y++)
                    {
                        LocalVoxelCoordinates blockLocation = new LocalVoxelCoordinates(x, y, z);
                        int blockId = chunk.GetBlockID(blockLocation);
                        if (blockId.Equals(AirBlock.BlockID))
                        {
                            chunk.SetBlockID(blockLocation, biome.WaterBlock);
                            var below = new LocalVoxelCoordinates(blockLocation.X, blockLocation.Y - 1, blockLocation.Z);
                            if (!chunk.GetBlockID(below).Equals(AirBlock.BlockID) && !chunk.GetBlockID(below).Equals(biome.WaterBlock))
                            {
                                if (!biome.WaterBlock.Equals(LavaBlock.BlockID) && !biome.WaterBlock.Equals(StationaryLavaBlock.BlockID))
                                {
                                    var random = new Random(seed);
                                    if (random.Next(100) < 40)
                                    {
                                        chunk.SetBlockID(below, ClayBlock.BlockID);
                                    }
                                    else
                                    {
                                        chunk.SetBlockID(below, SandBlock.BlockID);
                                    }
                                }
                            }
                        }
                    }
                    for (int y = 4; y < height / 8; y++)
                    {
                        LocalVoxelCoordinates blockLocation = new LocalVoxelCoordinates(x, y, z);
                        int blockId = chunk.GetBlockID(blockLocation);
                        if (blockId.Equals(AirBlock.BlockID))
                        {
                            chunk.SetBlockID(blockLocation, LavaBlock.BlockID);
                        }
                    }
                }
            }
        }
    }
}