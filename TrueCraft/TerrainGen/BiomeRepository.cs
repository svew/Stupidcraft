using System;
using System.Collections.Generic;
using System.Linq;
using TrueCraft.TerrainGen.Biomes;
using TrueCraft.Core.World;

namespace TrueCraft.TerrainGen
{
    public class BiomeRepository : IBiomeRepository
    {
        private readonly IBiomeProvider[] BiomeProviders = new IBiomeProvider[0x100];

        public BiomeRepository()
        {
            DiscoverBiomes();
        }

        private void DiscoverBiomes()
        {
            // TODO: this will only load Biomes from already loaded Assemblies.
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes().Where(t => typeof(IBiomeProvider).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    IBiomeProvider? instance = (IBiomeProvider?)Activator.CreateInstance(type);
                    if (instance is not null)
                        RegisterBiomeProvider(instance);
                }
            }
        }

        public void RegisterBiomeProvider(IBiomeProvider provider)
        {
            BiomeProviders[provider.ID] = provider;
        }

        public IBiomeProvider GetBiome(Biome id)
        {
            return BiomeProviders[(byte)id];
        }

        public IBiomeProvider GetBiome(double temperature, double rainfall, bool spawn)
        {
            List<IBiomeProvider> temperatureResults = new List<IBiomeProvider>();
            foreach (var biome in BiomeProviders)
            {
                if (biome != null && biome.Temperature.Equals(temperature))
                {
                    temperatureResults.Add(biome);
                }
            }

            if (temperatureResults.Count.Equals(0))
            {
                IBiomeProvider? provider = null;
                float temperatureDifference = 100.0f;
                foreach (var biome in BiomeProviders)
                {
                    if (biome != null)
                    {
                        var Difference = Math.Abs(temperature - biome.Temperature);
                        if (provider == null || Difference < temperatureDifference)
                        {
                            provider = biome;
                            temperatureDifference = (float)Difference;
                        }
                    }
                }
                if (provider is not null)
                    temperatureResults.Add(provider);
            }

            foreach (var biome in BiomeProviders)
            {
                if (biome != null
                    && biome.Rainfall.Equals(rainfall)
                    && temperatureResults.Contains(biome)
                    && (!spawn || biome.Spawn))
                {
                    return biome;
                }
            }

            IBiomeProvider? biomeProvider = null;
            float rainfallDifference = 100.0f;
            foreach (var biome in BiomeProviders)
            {
                if (biome != null)
                {
                    // TODO: why take the difference in temperature when we are checking Rainfall?
                    var difference = Math.Abs(temperature - biome.Temperature);
                    if ((biomeProvider == null || difference < rainfallDifference)
                        && (!spawn || biome.Spawn))
                    {
                        biomeProvider = biome;
                        rainfallDifference = (float)difference;
                    }
                }
            }
            return biomeProvider ?? new PlainsBiome();
        }
    }
}
