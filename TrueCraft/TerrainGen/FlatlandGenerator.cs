using System;
using System.Collections.Generic;
using TrueCraft.Core.World;
using TrueCraft.World;

namespace TrueCraft.TerrainGen
{
    public class FlatlandGenerator : Generator
    {
        private const string DefaultGeneratorOptions = "1;7,2x3,2;1";

        private readonly List<GeneratorLayer> _layers;
        private readonly string _generatorOptions;

        public FlatlandGenerator(int seed) : base(seed)
        {
            _generatorOptions = DefaultGeneratorOptions;
            _layers = new List<GeneratorLayer>();
            CreateLayers();
        }

        private void CreateLayers()
        {
            string[] parts = _generatorOptions.Split(';');
            string[] layers = parts[1].Split(',');
            double y = 0;
            foreach (var layer in layers)
            {
                var generatorLayer = new GeneratorLayer(layer);
                y += generatorLayer.Height;
                _layers.Add(generatorLayer);
            }
            Biome = (Biome)byte.Parse(parts[2]);
        }

        public Biome Biome { get; set; }

        public override IChunk GenerateChunk(GlobalChunkCoordinates position)
        {
            var chunk = new Chunk(position);
            int y = 0;
            for (int i = 0; i < _layers.Count; i++)
            {
                int height = y + _layers[i].Height;
                while (y < height)
                {
                    for (int x = 0; x < Chunk.Width; x++)
                    {
                        for (int z = 0; z < Chunk.Depth; z++)
                        {
                            LocalVoxelCoordinates local = new LocalVoxelCoordinates(x, y, z);
                            chunk.SetBlockID(local, _layers[i].BlockId);
                            chunk.SetMetadata(local, _layers[i].Metadata);
                        }
                    }
                    y++;
                }
            }
            for (int x = 0; x < Chunk.Width; x++)
                for (int z = 0; z < Chunk.Depth; z++)
                    chunk.SetBiome(x, z, Biome);
            chunk.TerrainPopulated = true;
            chunk.UpdateHeightMap();
            return chunk;
        }

        public string LevelType
        {
            get { return "FLAT"; }
        }

        public string GeneratorName { get { return "FLAT"; } }

        public long Seed { get; set; }

        public override GlobalVoxelCoordinates GetSpawn(IDimension dimension)
        {
            return new GlobalVoxelCoordinates(0, 5, 0);
        }

        protected class GeneratorLayer
        {
            public GeneratorLayer(string layer)
            {
                var parts = layer.Split('x');
                int idIndex = 0;
                if (parts.Length == 2)
                    idIndex++;
                var idParts = parts[idIndex].Split(':');
                BlockId = byte.Parse(idParts[0]);
                if (idParts.Length == 2)
                    Metadata = (byte)(byte.Parse(idParts[1]) & 0xF);
                Height = 1;
                if (parts.Length == 2)
                    Height = int.Parse(parts[0]);
            }

            public byte BlockId { get; set; }
            public byte Metadata { get; set; }
            public int Height { get; set; }
        }
    }
}
