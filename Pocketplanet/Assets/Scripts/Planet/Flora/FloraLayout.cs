using System;
using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public sealed class FloraLayoutSettings
    {
        public float GroveThreshold { get; set; } = 0.55f;
        public float GroveNoiseCells { get; set; } = 20f;
        public float MinScale { get; set; } = 0.8f;
        public float MaxScale { get; set; } = 1.2f;
    }

    public static class FloraLayout
    {
        public static List<FloraPlacement> Generate(SurfaceProfile profile, BiomeTable biomes,
            IReadOnlyList<FloraRule> rules, int seed, FloraLayoutSettings settings)
        {
            var rng = new System.Random(seed);
            var groves = new SeededNoise(seed);
            var placements = new List<FloraPlacement>();
            var ruleByBiome = RuleIndexByBiome(biomes, rules);

            for (int i = 0; i < profile.SampleCount; i++)
            {
                var sample = profile.SampleAtIndex(i);
                if (sample.Terrain != TerrainKind.Land || !ruleByBiome.TryGetValue(sample.Biome, out var ruleIndex))
                    continue;
                var arc = (i + (float)rng.NextDouble()) * profile.Spacing;
                var chance = rules[ruleIndex].Density * profile.Spacing * GroveStrength(groves, arc, profile.Circumference, settings);
                if (rng.NextDouble() >= chance)
                    continue;
                placements.Add(Plant(rng, arc, ruleIndex, rules[ruleIndex], settings));
            }
            return placements;
        }

        static FloraPlacement Plant(System.Random rng, float arc, int ruleIndex, FloraRule rule, FloraLayoutSettings settings)
        {
            var rows = SurfaceRows.FrontToBack;
            var row = rows[rng.Next(rows.Count)];
            var variant = rng.Next(Math.Max(1, rule.VariantCount));
            var scale = Mathf.Lerp(settings.MinScale, settings.MaxScale, (float)rng.NextDouble());
            return new FloraPlacement(arc, row, ruleIndex, variant, scale, rng.Next(2) == 0);
        }

        // Noise is sampled on a circle so groves join up across the arc seam.
        // Strength is 0 outside groves and rises toward each grove's heart, so trees thin out at the edges.
        static float GroveStrength(SeededNoise noise, float arc, float circumference, FloraLayoutSettings settings)
        {
            var angle = arc / circumference * 2f * Mathf.PI;
            var radius = settings.GroveNoiseCells / (2f * Mathf.PI);
            var value = noise.Value(1000f + Mathf.Cos(angle) * radius, 1000f + Mathf.Sin(angle) * radius);
            return Mathf.Clamp01((value - settings.GroveThreshold) / (1f - settings.GroveThreshold)) * 2f;
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
