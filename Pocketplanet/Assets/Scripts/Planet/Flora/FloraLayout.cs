using System;
using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public sealed class FloraLayoutSettings
    {
        public float Jitter { get; set; } = 0.45f;
        public float MinScale { get; set; } = 0.8f;
        public float MaxScale { get; set; } = 1.2f;
        public int RegrowthAttempts { get; set; } = 12;
    }

    public static class FloraLayout
    {
        // Walks the rim and drops a tree every 1/density units (with some jitter) wherever the biome has a rule,
        // which spreads trees evenly instead of clumping them.
        public static List<FloraPlacement> Generate(SurfaceProfile profile, BiomeTable biomes,
            IReadOnlyList<FloraRule> rules, int seed, FloraLayoutSettings settings)
        {
            var rng = new System.Random(seed);
            var placements = new List<FloraPlacement>();
            var ruleByBiome = RuleIndexByBiome(biomes, rules);

            var arc = (float)rng.NextDouble() * profile.Spacing;
            while (arc < profile.Circumference)
            {
                if (TryRuleAt(profile, ruleByBiome, arc, out var ruleIndex) && rules[ruleIndex].Density > 0f)
                {
                    placements.Add(Plant(rng, arc, ruleIndex, rules[ruleIndex], settings));
                    arc += Step(rng, rules[ruleIndex].Density, settings);
                }
                else
                {
                    arc += profile.Spacing;
                }
            }
            return placements;
        }

        public static FloraPlacement? FindRegrowthSpot(SurfaceProfile profile, BiomeTable biomes, IReadOnlyList<FloraRule> rules,
            System.Random rng, FloraLayoutSettings settings, Func<float, SurfaceRow, bool> isFree)
        {
            var ruleByBiome = RuleIndexByBiome(biomes, rules);
            for (int attempt = 0; attempt < settings.RegrowthAttempts; attempt++)
            {
                var arc = (float)rng.NextDouble() * profile.Circumference;
                if (!TryRuleAt(profile, ruleByBiome, arc, out var ruleIndex) || rules[ruleIndex].Density <= 0f)
                    continue;
                var placement = Plant(rng, arc, ruleIndex, rules[ruleIndex], settings);
                if (isFree(placement.Arc, placement.Row))
                    return placement;
            }
            return null;
        }

        static bool TryRuleAt(SurfaceProfile profile, Dictionary<byte, int> ruleByBiome, float arc, out int ruleIndex)
        {
            var sample = profile.SampleAt(arc);
            ruleIndex = -1;
            return sample.Terrain == TerrainKind.Land && ruleByBiome.TryGetValue(sample.Biome, out ruleIndex);
        }

        static float Step(System.Random rng, float density, FloraLayoutSettings settings) =>
            1f / density * Mathf.Lerp(1f - settings.Jitter, 1f + settings.Jitter, (float)rng.NextDouble());

        static FloraPlacement Plant(System.Random rng, float arc, int ruleIndex, FloraRule rule, FloraLayoutSettings settings)
        {
            var rows = SurfaceRows.FrontToBack;
            var row = rows[rng.Next(rows.Count)];
            var variant = rng.Next(Math.Max(1, rule.VariantCount));
            var scale = Mathf.Lerp(settings.MinScale, settings.MaxScale, (float)rng.NextDouble());
            return new FloraPlacement(arc, row, ruleIndex, variant, scale, rng.Next(2) == 0);
        }

        static Dictionary<byte, int> RuleIndexByBiome(BiomeTable biomes, IReadOnlyList<FloraRule> rules)
        {
            var map = new Dictionary<byte, int>();
            for (int b = 0; b < biomes.Count; b++)
                for (int r = 0; r < rules.Count; r++)
                    if (biomes[(byte)b].Id == rules[r].BiomeId)
                        map[(byte)b] = r;
            return map;
        }
    }
}
