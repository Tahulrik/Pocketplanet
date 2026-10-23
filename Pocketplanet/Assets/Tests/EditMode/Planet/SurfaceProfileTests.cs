using System.Linq;
using NUnit.Framework;

namespace PocketPlanet.Planet.Tests
{
    public class SurfaceProfileTests
    {
        static int CountLandRuns(SurfaceProfile profile)
        {
            var runs = 0;
            for (int i = 0; i < profile.SampleCount; i++)
            {
                var current = profile.SampleAtIndex(i).Terrain;
                var previous = profile.SampleAtIndex((i - 1 + profile.SampleCount) % profile.SampleCount).Terrain;
                if (current == TerrainKind.Land && previous == TerrainKind.Water)
                    runs++;
            }
            var allLand = Enumerable.Range(0, profile.SampleCount).All(i => profile.SampleAtIndex(i).Terrain == TerrainKind.Land);
            return allLand ? 1 : runs;
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void LandFraction_WithinTolerance(int seed)
        {
            var settings = TestPlanets.Settings(seed, circumference: 120f);
            var profile = TestPlanets.Profile(settings);
            var land = Enumerable.Range(0, profile.SampleCount).Count(i => profile.SampleAtIndex(i).Terrain == TerrainKind.Land);
            var tolerance = 0.03f + settings.ContinentCount * 2f * settings.BlockSize / settings.Circumference;
            Assert.AreEqual(0.6f, land / (float)profile.SampleCount, tolerance);
        }

        [Test]
        public void LandSegments_EqualContinentCount(
            [Values(1, 3, 8)] int continents, [Values(1, 2, 3)] int seed)
        {
            var profile = TestPlanets.Profile(TestPlanets.Settings(seed, circumference: 120f, continents: continents));
            Assert.AreEqual(continents, CountLandRuns(profile));
        }

        [Test]
        public void Segments_CoverWholeCircumference_AndNeighboursDiffer()
        {
            var profile = TestPlanets.Profile(TestPlanets.Settings(2));
            Assert.AreEqual(profile.Circumference, profile.Segments.Sum(s => s.Range.Length), 1e-3);
            for (int i = 0; i < profile.Segments.Count; i++)
            {
                var a = profile.Segments[i];
                var b = profile.Segments[(i + 1) % profile.Segments.Count];
                Assert.IsTrue(a.Terrain != b.Terrain || a.Biome != b.Biome);
            }
        }

        [Test]
        public void CoastFlag_SetOnlyAtTerrainChange()
        {
            var profile = TestPlanets.Profile(TestPlanets.Settings(2));
            var n = profile.SampleCount;
            for (int i = 0; i < n; i++)
            {
                var sample = profile.SampleAtIndex(i);
                var expected = profile.SampleAtIndex((i - 1 + n) % n).Terrain != sample.Terrain
                               || profile.SampleAtIndex((i + 1) % n).Terrain != sample.Terrain;
                Assert.AreEqual(expected, sample.IsCoast, $"sample {i}");
            }
        }

        [Test]
        public void SampleAt_MatchesSampleIndex()
        {
            var profile = TestPlanets.Profile(TestPlanets.Settings(2));
            var arc = 3.5f * profile.Spacing;
            Assert.AreEqual(profile.SampleAtIndex(3).Terrain, profile.TerrainAt(arc));
            Assert.AreEqual(profile.SampleAtIndex(3).Biome, profile.BiomeAt(arc));
        }

        [Test]
        public void SamplesIn_TinyRange_ReturnsCenterSample()
        {
            var profile = TestPlanets.Profile(TestPlanets.Settings(2));
            Assert.AreEqual(1, profile.SamplesIn(ArcRange.Centered(10f, 0.01f, profile.Circumference)).Count());
        }

        [Test]
        public void IsAll_AcrossSeam_ChecksBothSides()
        {
            var samples = new SurfaceSample[10];
            for (int i = 0; i < 10; i++)
                samples[i] = new SurfaceSample(i == 9 ? TerrainKind.Water : TerrainKind.Land, 0, false);
            var profile = new SurfaceProfile(samples, 1f);
            Assert.IsFalse(profile.IsAll(ArcRange.Centered(0f, 2f, 10f), s => s.Terrain == TerrainKind.Land));
            Assert.IsTrue(profile.IsAll(ArcRange.Centered(4f, 2f, 10f), s => s.Terrain == TerrainKind.Land));
        }
    }
}
