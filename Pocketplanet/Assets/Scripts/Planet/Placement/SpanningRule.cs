using System.Linq;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public class SpanningRule : IPlacementRule
    {
        readonly float maxSpan;

        public SpanningRule(float maxSpan)
        {
            this.maxSpan = maxSpan;
        }

        public PlacementReason Evaluate(ArcRange range, SurfaceProfile profile, BiomeTable biomes)
        {
            var samples = profile.SamplesIn(range).ToList();
            if (samples[0].Terrain == TerrainKind.Water || samples[samples.Count - 1].Terrain == TerrainKind.Water)
                return PlacementReason.Water;
            if (samples.Any(s => s.Terrain == TerrainKind.Land && !biomes[s.Biome].Buildable))
                return PlacementReason.BiomeNotBuildable;
            return LongestWaterRun(samples) * profile.Spacing > maxSpan ? PlacementReason.RuleViolation : PlacementReason.None;
        }

        static int LongestWaterRun(System.Collections.Generic.List<SurfaceSample> samples)
        {
            int longest = 0, current = 0;
            foreach (var sample in samples)
            {
                current = sample.Terrain == TerrainKind.Water ? current + 1 : 0;
                longest = Mathf.Max(longest, current);
            }
            return longest;
        }
    }
}
