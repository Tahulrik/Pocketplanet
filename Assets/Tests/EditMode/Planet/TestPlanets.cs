using UnityEngine;

namespace PocketPlanet.Planet.Tests
{
    public static class TestPlanets
    {
        public static PlanetSettings Settings(int seed = 1, float circumference = 48f, int continents = 3, float coverage = 0.6f)
        {
            var settings = ScriptableObject.CreateInstance<PlanetSettings>();
            settings.Seed = seed;
            settings.Circumference = circumference;
            settings.ContinentCount = continents;
            settings.LandCoverage = coverage;
            settings.BiomeTable = DefaultBiomes.Create();
            return settings;
        }

        public static SurfaceProfile Profile(PlanetSettings settings)
        {
            var biomes = new BiomeTable(settings.BiomeTable);
            var arcs = RimLayout.BuildLandArcs(settings, new System.Random(settings.Seed));
            var map = BlockMapGenerator.Generate(settings, biomes, arcs, settings.Seed);
            return SurfaceProfile.Build(map, biomes, settings.Radius, settings.SampleSpacing);
        }
    }
}
