using System;

namespace PocketPlanet.Planet
{
    public class PlanetQueries
    {
        readonly SurfaceLayer surface;
        readonly BiomeTable biomes;

        public PlanetQueries(SurfaceLayer surface, BiomeTable biomes)
        {
            this.surface = surface;
            this.biomes = biomes;
        }

        SurfaceProfile Profile => surface.Profile;

        public ArcRange? FindLandingSite(float nearArc, float length, bool allowWater)
        {
            foreach (var center in OutwardFrom(nearArc))
            {
                var range = ArcRange.Centered(center, length, Profile.Circumference);
                if (IsFreeLand(range) || (allowWater && Profile.IsAll(range, IsWater)))
                    return range;
            }
            return null;
        }

        public float? NearestCoast(float arc) => NearestSample(arc, s => s.IsCoast);

        public float? FindNearestDeepWater(float arc) => NearestSample(arc, s => s.Biome == biomes.DeepWater);

        float? NearestSample(float arc, Func<SurfaceSample, bool> predicate)
        {
            foreach (var candidate in OutwardFrom(arc))
                if (predicate(Profile.SampleAt(candidate)))
                    return SampleCenter(candidate);
            return null;
        }

        bool IsFreeLand(ArcRange range) =>
            Profile.IsAll(range, s => s.Terrain == TerrainKind.Land)
            && surface.IsFreeInAllRows(range);

        static bool IsWater(SurfaceSample sample) => sample.Terrain == TerrainKind.Water;

        float SampleCenter(float arc) => (Profile.IndexOf(arc) + 0.5f) * Profile.Spacing;

        System.Collections.Generic.IEnumerable<float> OutwardFrom(float arc)
        {
            yield return arc;
            var steps = Profile.SampleCount / 2;
            for (int step = 1; step <= steps; step++)
            {
                yield return ArcRange.Wrap(arc + step * Profile.Spacing, Profile.Circumference);
                yield return ArcRange.Wrap(arc - step * Profile.Spacing, Profile.Circumference);
            }
        }
    }
}
