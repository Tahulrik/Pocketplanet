using System.Linq;
using NUnit.Framework;

namespace PocketPlanet.Planet.Tests
{
    public class PlanetQueriesTests
    {
        static bool IsLand(SurfaceSample s) => s.Terrain == TerrainKind.Land;
        static bool IsWater(SurfaceSample s) => s.Terrain == TerrainKind.Water;

        [Test]
        public void LandingSite_IsLandAndFreeInBothRows()
        {
            var planet = TestPlanets.Generate(3);
            var site = planet.Queries.FindLandingSite(0f, 2f, false);
            Assert.IsTrue(site.HasValue);
            Assert.IsTrue(planet.Surface.Profile.IsAll(site.Value, IsLand));
            Assert.IsTrue(planet.Surface.Row(SurfaceRow.Front).IsFree(site.Value));
            Assert.IsTrue(planet.Surface.Row(SurfaceRow.Back).IsFree(site.Value));
        }

        [Test]
        public void LandingSite_SkipsOccupied()
        {
            var planet = TestPlanets.Generate(3);
            var first = planet.Queries.FindLandingSite(0f, 2f, false).Value;
            planet.Surface.Row(SurfaceRow.Front).TryAdd(new FakeOccupant("house"), first);
            var second = planet.Queries.FindLandingSite(first.Center, 2f, false);
            Assert.IsTrue(second.HasValue);
            Assert.IsFalse(second.Value.Overlaps(first));
        }

        [Test]
        public void LandingSite_None_ReturnsNull()
        {
            var planet = TestPlanets.Generate(3);
            Assert.IsNull(planet.Queries.FindLandingSite(0f, LongestLandRun(planet.Surface.Profile) + 1f, false));
        }

        static float LongestLandRun(SurfaceProfile profile)
        {
            int longest = 0, current = 0;
            for (int i = 0; i < profile.SampleCount * 2; i++)
            {
                current = IsLand(profile.SampleAtIndex(i % profile.SampleCount)) ? current + 1 : 0;
                longest = System.Math.Max(longest, System.Math.Min(current, profile.SampleCount));
            }
            return longest * profile.Spacing;
        }

        [Test]
        public void LandingSite_AllowWater_FindsWaterNearOcean()
        {
            var planet = TestPlanets.Generate(3);
            var ocean = planet.Surface.Profile.Segments.Where(s => s.Terrain == TerrainKind.Water)
                .OrderByDescending(s => s.Range.Length).First();
            var site = planet.Queries.FindLandingSite(ocean.Range.Center, 1f, true);
            Assert.IsTrue(site.HasValue);
            Assert.IsTrue(planet.Surface.Profile.IsAll(site.Value, IsWater));
        }

        [Test]
        public void NearestCoast_ReturnsCoastSampleArc()
        {
            var planet = TestPlanets.Generate(3);
            var coast = planet.Queries.NearestCoast(5f);
            Assert.IsTrue(coast.HasValue);
            Assert.IsTrue(planet.Surface.Profile.SampleAt(coast.Value).IsCoast);
        }

        [Test]
        public void FindNearestDeepWater_ReturnsDeepWaterArc()
        {
            var planet = TestPlanets.Generate(3);
            var deep = planet.Queries.FindNearestDeepWater(5f);
            Assume.That(deep.HasValue, "No deep water on the rim for this seed.");
            Assert.AreEqual(planet.Biomes.DeepWater, planet.Surface.Profile.BiomeAt(deep.Value));
        }
    }
}
