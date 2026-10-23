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
    }
}
