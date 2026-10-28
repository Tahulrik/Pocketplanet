using System.Linq;
using NUnit.Framework;

namespace PocketPlanet.Planet.Tests
{
    public class BiomeSpreadTests
    {
        static bool HasOnRim(PlanetModel planet, string biomeId) =>
            Enumerable.Range(0, planet.Surface.Profile.SampleCount)
                .Select(i => planet.Surface.Profile.SampleAtIndex(i))
                .Any(s => s.Terrain == TerrainKind.Land && planet.Biomes[s.Biome].Id == biomeId);

        [TestCase("Desert")]
        [TestCase("Snow")]
        [TestCase("Forest")]
        public void Biome_ShowsUpOnTheRimOfMostPlanets(string biomeId)
        {
            var planetsWithBiome = Enumerable.Range(1, 10).Count(seed => HasOnRim(TestPlanets.Generate(seed), biomeId));
            Assert.GreaterOrEqual(planetsWithBiome, 5, $"{biomeId} appeared on only {planetsWithBiome} of 10 planets.");
        }
    }
}
