using System;
using TrueCraft.Core.TerrainGen.Decorations;
using TrueCraft.Core.TerrainGen.Noise;
using TrueCraft.Core.World;

namespace TrueCraft.Core.TerrainGen.Decorators
{
    public class DungeonDecorator : IChunkDecorator
    {
        private int BaseLevel;

        public DungeonDecorator(int groundLevel)
        {
            this.BaseLevel = groundLevel;
        }

        public void Decorate(IDimension dimension, IChunk chunk, IBiomeRepository biomes)
        {
            for (int attempts = 0; attempts < 8; attempts++)
            {
                var noise = new Perlin(dimension.Seed - (chunk.Coordinates.X + chunk.Coordinates.Z));
                var offsetNoise = new ClampNoise(noise);
                offsetNoise.MaxValue = 3;
                var x = 0;
                var z = 0;
                var offset = 0.0;
                offset += offsetNoise.Value2D(x, z);
                int finalX = (int)Math.Floor(x + offset);
                int finalZ = (int)Math.Floor(z + offset);
                var y = (int)(10 + offset);

                var blockX = MathHelper.ChunkToBlockX(finalX, chunk.Coordinates.X);
                var blockZ = MathHelper.ChunkToBlockZ(finalZ, chunk.Coordinates.Z);
                var spawnValue = offsetNoise.Value2D(blockX, blockZ);
                if (spawnValue > 1.95 && spawnValue < 2.09)
                {
                    var generated = new Dungeon().GenerateAt(dimension, chunk, new LocalVoxelCoordinates(finalX, y, finalZ));
                    if (generated)
                        break;
                }
            }
        }
    }
}