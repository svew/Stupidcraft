using System;
using System.Linq;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.TerrainGen.Decorations;
using TrueCraft.Core.TerrainGen.Noise;
using TrueCraft.Core.World;

namespace TrueCraft.Core.TerrainGen.Decorators
{
    public class CactusDecorator : IChunkDecorator
    {
        public void Decorate(IDimension world, IChunk chunk, IBiomeRepository biomes)
        {
            var noise = new Perlin(world.Seed);
            var chanceNoise = new ClampNoise(noise);
            chanceNoise.MaxValue = 2;
            for (int x = 0; x < Chunk.Width; x++)
            {
                for (int z = 0; z < Chunk.Depth; z++)
                {
                    var biome = biomes.GetBiome(chunk.Biomes[x * Chunk.Width + z]);
                    var blockX = MathHelper.ChunkToBlockX(x, chunk.Coordinates.X);
                    var blockZ = MathHelper.ChunkToBlockZ(z, chunk.Coordinates.Z);

                    int height = chunk.GetHeight(x, z);
                    if (biome.Plants.Contains(PlantSpecies.Cactus) && chanceNoise.Value2D(blockX, blockZ) > 1.7)
                    {
                        var blockLocation = new LocalVoxelCoordinates(x, height, z);
                        var cactiPosition = new LocalVoxelCoordinates(blockLocation.X, blockLocation.Y + 1, blockLocation.Z);
                        if (chunk.GetBlockID(blockLocation).Equals(SandBlock.BlockID))
                        {
                            var HeightChance = chanceNoise.Value2D(blockX, blockZ);
                            var CactusHeight = (HeightChance < 1.4) ? 2 : 3;
                            Decoration.GenerateColumn(chunk, cactiPosition, CactusHeight, CactusBlock.BlockID);
                        }
                    }
                }
            }
        }
    }
}