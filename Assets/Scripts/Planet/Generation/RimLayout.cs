using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public static class RimLayout
    {
        const float MinBlocksPerArc = 3f;
        const float LengthJitter = 0.25f;

        public static IReadOnlyList<ArcRange> BuildLandArcs(PlanetSettings settings, System.Random rng)
        {
            var circumference = settings.Circumference;
            var minArc = MinBlocksPerArc * settings.BlockSize;
            var land = circumference * settings.LandCoverage;
            var ocean = circumference - land;
            var count = FittingContinentCount(settings.ContinentCount, land, ocean, minArc);

            var landLengths = Distribute(land, count, minArc, rng);
            var oceanLengths = Distribute(ocean, count, minArc, rng);

            var arcs = new List<ArcRange>(count);
            var cursor = (float)rng.NextDouble() * circumference;
            for (int i = 0; i < count; i++)
            {
                arcs.Add(new ArcRange(cursor, landLengths[i], circumference));
                cursor += landLengths[i] + oceanLengths[i];
            }
            return arcs.OrderBy(a => a.Start).ToList();
        }

        static int FittingContinentCount(int requested, float land, float ocean, float minArc)
        {
            var count = Mathf.Max(1, requested);
            while (count > 1 && (land < count * minArc || ocean < count * minArc))
                count--;
            if (count < requested)
                Debug.LogWarning($"ContinentCount clamped from {requested} to {count}: the planet is too small for more.");
            return count;
        }

        static float[] Distribute(float total, int count, float minimum, System.Random rng)
        {
            var lengths = new float[count];
            for (int i = 0; i < count; i++)
                lengths[i] = 1f + ((float)rng.NextDouble() * 2f - 1f) * LengthJitter;

            var scale = total / lengths.Sum();
            for (int i = 0; i < count; i++)
                lengths[i] *= scale;

            RaiseToMinimum(lengths, minimum);
            return lengths;
        }

        static void RaiseToMinimum(float[] lengths, float minimum)
        {
            for (int i = 0; i < lengths.Length; i++)
            {
                if (lengths[i] >= minimum)
                    continue;
                var deficit = minimum - lengths[i];
                lengths[i] = minimum;
                var largest = System.Array.IndexOf(lengths, lengths.Max());
                lengths[largest] -= deficit;
            }
        }
    }
}
