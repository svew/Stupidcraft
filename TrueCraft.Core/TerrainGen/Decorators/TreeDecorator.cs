using System;
using System.Linq;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Logic.Blocks;
using TrueCraft.Core.TerrainGen.Decorations;
using TrueCraft.Core.TerrainGen.Noise;
using TrueCraft.Core.World;

namespace TrueCraft.Core.TerrainGen.Decorators
{
    public class TreeDecorator : IChunkDecorator
    {
        public Perlin Noise { get; set; }
        public ClampNoise ChanceNoise { get; set; }

        public void Decorate(IDimension world, IChunk chunk, IBiomeRepository biomes)
        {
            Noise = new Perlin(world.Seed);
            ChanceNoise = new ClampNoise(Noise);
            ChanceNoise.MaxValue = 2;
            LocalColumnCoordinates lastTree = null;
            for (int x = 0; x < Chunk.Width; x++)
            {
                for (int z = 0; z < Chunk.Depth; z++)
                {
                    var biome = biomes.GetBiome(chunk.Biomes[x * Chunk.Width + z]);
                    var blockX = MathHelper.ChunkToBlockX(x, chunk.Coordinates.X);
                    var blockZ = MathHelper.ChunkToBlockZ(z, chunk.Coordinates.Z);
                    int height = chunk.GetHeight(x, z);

                    if (lastTree != null && lastTree.DistanceTo(new LocalColumnCoordinates(x, z)) < biome.TreeDensity)
                        continue;

                    if (Noise.Value2D(blockX, blockZ) > 0.3)
                    {
                        LocalVoxelCoordinates location = new LocalVoxelCoordinates(x, height, z);
                        byte id = chunk.GetBlockID(location);
                        IBlockProvider provider = world.BlockRepository.GetBlockProvider(id);
                        if (id == DirtBlock.BlockID || id == GrassBlock.BlockID || id == SnowfallBlock.BlockID
                            || (id != StationaryWaterBlock.BlockID && id != WaterBlock.BlockID
                                && id != LavaBlock.BlockID && id != StationaryLavaBlock.BlockID
                                && provider.BoundingBox == null))
                        {
                            if (provider.BoundingBox == null)
                                location = new LocalVoxelCoordinates(location.X, location.Y - 1, location.Z);
                            var oakNoise = ChanceNoise.Value2D(blockX * 0.6, blockZ * 0.6);
                            var birchNoise = ChanceNoise.Value2D(blockX * 0.2, blockZ * 0.2);
                            var spruceNoise = ChanceNoise.Value2D(blockX * 0.35, blockZ * 0.35);

                            LocalVoxelCoordinates baseCoordinates = new LocalVoxelCoordinates(location.X, location.Y + 1, location.Z);
                            if (biome.Trees.Contains(TreeSpecies.Oak) && oakNoise > 1.01 && oakNoise < 1.25)
                            {
                                var oak = new OakTree().GenerateAt(world, chunk, baseCoordinates);
                                if (oak)
                                {
                                    lastTree = new LocalColumnCoordinates(x, z);
                                    continue;
                                }
                            }
                            if (biome.Trees.Contains(TreeSpecies.Birch) && birchNoise > 0.3 && birchNoise < 0.95)
                            {
                                var birch = new BirchTree().GenerateAt(world, chunk, baseCoordinates);
                                if (birch)
                                {
                                    lastTree = new LocalColumnCoordinates(x, z);
                                    continue;
                                }
                            }
                            if (biome.Trees.Contains(TreeSpecies.Spruce) && spruceNoise < 0.75)
                            {
                                var random = new Random(world.Seed);
                                var type = random.Next(1, 2);
                                var generated = false;
                                if (type.Equals(1))
                                    generated = new PineTree().GenerateAt(world, chunk, baseCoordinates);
                                else
                                    generated = new ConiferTree().GenerateAt(world, chunk, baseCoordinates);

                                if (generated)
                                {
                                    lastTree = new LocalColumnCoordinates(x, z);
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}