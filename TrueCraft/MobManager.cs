using System;
using System.Collections.Generic;
using TrueCraft.Core;
using TrueCraft.Core.AI;
using TrueCraft.Core.World;

namespace TrueCraft
{
    public class MobManager
    {
        public EntityManager EntityManager { get; set; }

        private Dictionary<DimensionID, List<ISpawnRule>> SpawnRules { get; set; }

        public MobManager(EntityManager manager)
        {
            EntityManager = manager;
            SpawnRules = new Dictionary<DimensionID, List<ISpawnRule>>();
        }

        public void AddRules(DimensionID dimension, ISpawnRule rules)
        {
            if (!SpawnRules.ContainsKey(dimension))
                SpawnRules[dimension] = new List<ISpawnRule>();
            SpawnRules[dimension].Add(rules);
        }

        public void SpawnInitialMobs(IChunk chunk, DimensionID dimension)
        {
            if (!SpawnRules.ContainsKey(dimension))
                return;
            var rules = SpawnRules[dimension];
            foreach (var rule in rules)
            {
                if (MathHelper.Random.Next(rule.ChunkSpawnChance) == 0)
                    rule.GenerateMobs(chunk, EntityManager);
            }
        }

        /// <summary>
        /// Call at dusk and it'll spawn baddies.
        /// </summary>
        public void DayCycleSpawn(IChunk chunk, DimensionID dimension)
        {
            // TODO
        }
    }
}