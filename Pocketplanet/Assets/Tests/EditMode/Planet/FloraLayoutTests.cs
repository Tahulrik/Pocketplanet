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
        public void Trees_AreEvenlySpacedAcrossLand()
        {
            const float density = 2f;
            var planet = TestPlanets.Generate(4);
            var profile = planet.Surface.Profile;
            var placements = FloraLayout.Generate(profile, planet.Biomes, AllLand(planet, density), 9, Settings);
            var minStep = (1f - Settings.Jitter) / density - 1e-3f;
            var maxStep = (1f + Settings.Jitter) / density + 1e-3f;

            var checkedGaps = 0;
            for (int i = 1; i < placements.Count; i++)
            {
                var from = placements[i - 1].Arc;
                var gap = placements[i].Arc - from;
                if (!profile.IsAll(new ArcRange(from, gap, profile.Circumference), s => s.Terrain == TerrainKind.Land))
                    continue;
                Assert.That(gap, Is.InRange(minStep, maxStep));
                checkedGaps++;
            }
            Assert.Greater(checkedGaps, 10);
        }

        [Test]
        public void TreeCount_FollowsDensity()
        {
            var planet = TestPlanets.Generate(4);
            var landLength = planet.Surface.Profile.Segments.Where(s => s.Terrain == TerrainKind.Land).Sum(s => s.Range.Length);
            var count = FloraLayout.Generate(planet.Surface.Profile, planet.Biomes, AllLand(planet, 1f), 9, Settings).Count;
            Assert.That(count, Is.InRange(landLength * 0.75f, landLength * 1.25f));
        }

        [Test]
        public void RegrowthSpot_IsOnLand_AndAcceptedByCaller()
        {
            var planet = TestPlanets.Generate(4);
            var spot = FloraLayout.FindRegrowthSpot(planet.Surface.Profile, planet.Biomes, AllLand(planet, 1f),
                new System.Random(3), Settings, (_, row) => row == SurfaceRow.Back);
            Assert.IsTrue(spot.HasValue);
            Assert.AreEqual(SurfaceRow.Back, spot.Value.Row);
            Assert.AreEqual(TerrainKind.Land, planet.Surface.Profile.TerrainAt(spot.Value.Arc));
        }

        [Test]
        public void RegrowthSpot_NoneWhenNothingIsFree()
        {
            var planet = TestPlanets.Generate(4);
            var spot = FloraLayout.FindRegrowthSpot(planet.Surface.Profile, planet.Biomes, AllLand(planet, 1f),
                new System.Random(3), Settings, (_, __) => false);
            Assert.IsFalse(spot.HasValue);
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
