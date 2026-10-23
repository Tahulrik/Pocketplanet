using System;
using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public class SurfaceProfile
    {
        readonly SurfaceSample[] samples;
        readonly List<SurfaceSegment> segments;

        internal SurfaceProfile(SurfaceSample[] samples, float spacing)
        {
            this.samples = samples;
            Spacing = spacing;
            segments = BuildSegments();
        }

        public static SurfaceProfile Build(BlockMap map, BiomeTable biomes, float radius, float spacing)
        {
            var circumference = 2f * Mathf.PI * radius;
            var count = Mathf.Max(1, Mathf.RoundToInt(circumference / spacing));
            var exactSpacing = circumference / count;
            var ringRadius = radius - map.BlockSize / 2f;

            var samples = new SurfaceSample[count];
            for (int i = 0; i < count; i++)
            {
                var angle = (i + 0.5f) * exactSpacing / radius;
                var point = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ringRadius;
                var biome = BiomeUnder(map, biomes, point);
                var terrain = biomes.IsLand(biome) ? TerrainKind.Land : TerrainKind.Water;
                samples[i] = new SurfaceSample(terrain, biome, false);
            }
            MarkCoasts(samples);
            return new SurfaceProfile(samples, exactSpacing);
        }

        public float Spacing { get; }
        public int SampleCount => samples.Length;
        public float Circumference => samples.Length * Spacing;
        public IReadOnlyList<SurfaceSegment> Segments => segments;

        public SurfaceSample SampleAtIndex(int index) => samples[index];
        public SurfaceSample SampleAt(float arc) => samples[IndexOf(arc)];
        public TerrainKind TerrainAt(float arc) => SampleAt(arc).Terrain;
        public byte BiomeAt(float arc) => SampleAt(arc).Biome;

        public int IndexOf(float arc) =>
            Mathf.Min(samples.Length - 1, Mathf.FloorToInt(ArcRange.Wrap(arc, Circumference) / Spacing));

        public IEnumerable<SurfaceSample> SamplesIn(ArcRange range)
        {
            var any = false;
            var first = IndexOf(range.Start);
            var steps = Mathf.CeilToInt(range.Length / Spacing) + 1;
            for (int step = 0; step <= steps && step < samples.Length; step++)
            {
                var index = (first + step) % samples.Length;
                if (!range.Contains((index + 0.5f) * Spacing))
                    continue;
                any = true;
                yield return samples[index];
            }
            if (!any)
                yield return SampleAt(range.Center);
        }

        public bool IsAll(ArcRange range, Func<SurfaceSample, bool> predicate)
        {
            foreach (var sample in SamplesIn(range))
                if (!predicate(sample))
                    return false;
            return true;
        }

        static byte BiomeUnder(BlockMap map, BiomeTable biomes, Vector2 point)
        {
            if (!map.TryGetBlock(point, out var x, out var y) || map[x, y] == BlockMap.Empty)
                return biomes.DeepWater;
            return map[x, y];
        }

        static void MarkCoasts(SurfaceSample[] samples)
        {
            var n = samples.Length;
            var terrains = Array.ConvertAll(samples, s => s.Terrain);
            for (int i = 0; i < n; i++)
            {
                var coast = terrains[(i - 1 + n) % n] != terrains[i] || terrains[(i + 1) % n] != terrains[i];
                samples[i] = samples[i].WithCoast(coast);
            }
        }

        List<SurfaceSegment> BuildSegments()
        {
            var n = samples.Length;
            var start = FirstSegmentStart();
            if (start < 0)
                return new List<SurfaceSegment>
                {
                    new SurfaceSegment(new ArcRange(0f, Circumference, Circumference), samples[0].Terrain, samples[0].Biome)
                };

            var result = new List<SurfaceSegment>();
            var runStart = start;
            for (int step = 1; step <= n; step++)
            {
                var index = (start + step) % n;
                if (step < n && SameKind(samples[index], samples[runStart]))
                    continue;
                var length = ((index - runStart + n) % n == 0 ? n : (index - runStart + n) % n) * Spacing;
                result.Add(new SurfaceSegment(new ArcRange(runStart * Spacing, length, Circumference),
                    samples[runStart].Terrain, samples[runStart].Biome));
                runStart = index;
            }
            return result;
        }

        int FirstSegmentStart()
        {
            for (int i = 0; i < samples.Length; i++)
                if (!SameKind(samples[i], samples[(i - 1 + samples.Length) % samples.Length]))
                    return i;
            return -1;
        }

        static bool SameKind(SurfaceSample a, SurfaceSample b) => a.Terrain == b.Terrain && a.Biome == b.Biome;
    }
}
