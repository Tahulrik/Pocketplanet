using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public static class BlockMapGenerator
    {
        const float LandThreshold = 0.5f;
        const float InlandReach = 0.45f;
        const float CoastNoiseScale = 0.15f;
        const float IslandNoiseScale = 0.25f;
        const float IslandThreshold = 0.78f;
        const float IslandBonus = 0.6f;
        const float ClimateNoiseScale = 0.08f;
        const int Octaves = 4;

        public static BlockMap Generate(PlanetSettings settings, BiomeTable biomes, IReadOnlyList<ArcRange> landArcs, int seed)
        {
            var map = new BlockMap(settings.GridSize, settings.BlockSize);
            var noise = new SeededNoise(seed);
            var inside = new bool[map.Size, map.Size];
            var land = new bool[map.Size, map.Size];

            MarkLandmass(settings, landArcs, map, noise, inside, land);
            ForceRimToLayout(settings, landArcs, map, inside, land);
            AssignBiomes(biomes, map, noise, inside, land);
            return map;
        }

        static void MarkLandmass(PlanetSettings settings, IReadOnlyList<ArcRange> landArcs, BlockMap map,
            SeededNoise noise, bool[,] inside, bool[,] land)
        {
            var radius = settings.Radius;
            for (int x = 0; x < map.Size; x++)
                for (int y = 0; y < map.Size; y++)
                {
                    var p = map.BlockCenter(x, y);
                    inside[x, y] = p.magnitude <= radius;
                    if (inside[x, y])
                        land[x, y] = LandScore(settings, landArcs, noise, p) > LandThreshold;
                }
        }

        static float LandScore(PlanetSettings settings, IReadOnlyList<ArcRange> landArcs, SeededNoise noise, Vector2 p)
        {
            var radius = settings.Radius;
            var inland = 1f - (radius - p.magnitude) / (InlandReach * radius);
            var arc = ArcOf(p, radius, settings.Circumference);

            var continent = 0f;
            foreach (var landArc in landArcs)
                continent = Mathf.Max(continent, ArcMembership(landArc, arc, settings.BlockSize) * inland);

            var coast = (noise.Fractal(p.x * CoastNoiseScale, p.y * CoastNoiseScale, Octaves) - 0.5f) * settings.CoastRoughness;
            var island = noise.Fractal(p.x * IslandNoiseScale + 100f, p.y * IslandNoiseScale + 100f, Octaves) > IslandThreshold
                ? IslandBonus
                : 0f;
            return continent + coast + island;
        }

        static float ArcMembership(ArcRange landArc, float arc, float blockSize)
        {
            if (landArc.Contains(arc))
                return 1f;
            var c = landArc.Circumference;
            var distance = Mathf.Min(ArcRange.Distance(arc, landArc.Start, c), ArcRange.Distance(arc, landArc.End, c));
            return Mathf.Max(0f, 1f - distance / (2f * blockSize));
        }

        static void ForceRimToLayout(PlanetSettings settings, IReadOnlyList<ArcRange> landArcs, BlockMap map,
            bool[,] inside, bool[,] land)
        {
            var radius = settings.Radius;
            var ringRadius = radius - settings.BlockSize / 2f;
            var samples = Mathf.RoundToInt(settings.Circumference / settings.SampleSpacing);
            for (int i = 0; i < samples; i++)
            {
                var angle = (i + 0.5f) * settings.SampleSpacing / radius;
                var point = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ringRadius;
                if (!map.TryGetBlock(point, out var x, out var y))
                    continue;
                var blockArc = ArcOf(map.BlockCenter(x, y), radius, settings.Circumference);
                inside[x, y] = true;
                land[x, y] = IsInAny(landArcs, blockArc);
            }
        }

        static void AssignBiomes(BiomeTable biomes, BlockMap map, SeededNoise noise, bool[,] inside, bool[,] land)
        {
            for (int x = 0; x < map.Size; x++)
                for (int y = 0; y < map.Size; y++)
                {
                    if (!inside[x, y])
                        continue;
                    if (land[x, y])
                        map[x, y] = ClassifyLand(biomes, noise, map.BlockCenter(x, y));
                    else
                        map[x, y] = HasLandNeighbour(land, x, y) ? biomes.ShallowWater : biomes.DeepWater;
                }
        }

        static byte ClassifyLand(BiomeTable biomes, SeededNoise noise, Vector2 p)
        {
            var temperature = noise.Fractal(p.x * ClimateNoiseScale + 300f, p.y * ClimateNoiseScale + 300f, Octaves);
            var moisture = noise.Fractal(p.x * ClimateNoiseScale + 600f, p.y * ClimateNoiseScale + 600f, Octaves);
            return biomes.ClassifyLand(temperature, moisture);
        }

        static bool HasLandNeighbour(bool[,] land, int x, int y)
        {
            var size = land.GetLength(0);
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + dx, ny = y + dy;
                    if ((dx != 0 || dy != 0) && nx >= 0 && ny >= 0 && nx < size && ny < size && land[nx, ny])
                        return true;
                }
            return false;
        }

        static bool IsInAny(IReadOnlyList<ArcRange> arcs, float arc)
        {
            foreach (var range in arcs)
                if (range.Contains(arc))
                    return true;
            return false;
        }

        static float ArcOf(Vector2 p, float radius, float circumference) =>
            ArcRange.Wrap(Mathf.Atan2(p.y, p.x) * radius, circumference);
    }
}
