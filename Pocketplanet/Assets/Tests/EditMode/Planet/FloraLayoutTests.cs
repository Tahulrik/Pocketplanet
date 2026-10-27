using System.Linq;
using NUnit.Framework;

namespace PocketPlanet.Planet.Tests
{
    public class FloraLayoutTests
    {
        static readonly FloraLayoutSettings Settings = new FloraLayoutSettings();

        static FloraRule[] AllLand(PlanetModel planet, float density) =>
            Enumerable.Range(0, planet.Biomes.Count)
                .Select(i => planet.Biomes[(byte)i])
                .Where(b => b.Kind == BiomeKind.Land)
                .Select(b => new FloraRule(b.Id, density, 3))
                .ToArray();

        [Test]
        public void SameSeed_SameLayout()
        {
            var planet = TestPlanets.Generate(4);
            var rules = AllLand(planet, 3f);
            var a = FloraLayout.Generate(planet.Surface.Profile, planet.Biomes, rules, 9, Settings);
            var b = FloraLayout.Generate(planet.Surface.Profile, planet.Biomes, rules, 9, Settings);
            CollectionAssert.AreEqual(a.Select(p => p.Arc), b.Select(p => p.Arc));
        }

        [Test]
        public void Trees_OnlyGrowOnLand()
        {
            var planet = TestPlanets.Generate(4);
            var placements = FloraLayout.Generate(planet.Surface.Profile, planet.Biomes, AllLand(planet, 3f), 9, Settings);
            Assert.IsNotEmpty(placements);
            Assert.IsTrue(placements.All(p => planet.Surface.Profile.TerrainAt(p.Arc) == TerrainKind.Land));
        }

        [Test]
        public void BiomeWithoutRule_HasNoTrees()
        {
            var planet = TestPlanets.Generate(4);
            var placements = FloraLayout.Generate(planet.Surface.Profile, planet.Biomes, new FloraRule[0], 9, Settings);
            Assert.IsEmpty(placements);
        }

        [Test]
        public void Trees_ClusterIntoGroves()
        {
            var planet = TestPlanets.Generate(4);
            var landSamples = Enumerable.Range(0, planet.Surface.Profile.SampleCount)
                .Count(i => planet.Surface.Profile.SampleAtIndex(i).Terrain == TerrainKind.Land);
            var counts = Enumerable.Range(1, 6)
                .Select(seed => FloraLayout.Generate(planet.Surface.Profile, planet.Biomes, AllLand(planet, 40f), seed, Settings).Count)
                .ToArray();
            var averageCoverage = counts.Average() / landSamples;
            Assert.Greater(averageCoverage, 0.15, "Groves should be dense at high density.");
            Assert.Less(averageCoverage, 0.7, "Grove noise should leave open land between groves.");
        }

        [Test]
        public void Placements_UseAllRowsAndValidVariants()
        {
            var planet = TestPlanets.Generate(4);
            var placements = FloraLayout.Generate(planet.Surface.Profile, planet.Biomes, AllLand(planet, 4f), 9, Settings);
            CollectionAssert.IsSubsetOf(SurfaceRows.FrontToBack, placements.Select(p => p.Row).Distinct());
            Assert.IsTrue(placements.All(p => p.Variant >= 0 && p.Variant < 3));
            Assert.IsTrue(placements.All(p => p.Scale >= Settings.MinScale && p.Scale <= Settings.MaxScale));
        }
    }
}
