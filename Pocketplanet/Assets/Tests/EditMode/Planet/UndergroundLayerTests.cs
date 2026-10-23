using System.Linq;
using NUnit.Framework;

namespace PocketPlanet.Planet.Tests
{
    public class UndergroundLayerTests
    {
        static (UndergroundLayer layer, SurfaceProfile profile, BiomeTable biomes) Build(int seed = 2)
        {
            var settings = TestPlanets.Settings(seed, circumference: 120f);
            var profile = TestPlanets.Profile(settings);
            var biomes = new BiomeTable(settings.BiomeTable);
            return (UndergroundLayer.Build(profile, biomes, settings.UndergroundBands, seed), profile, biomes);
        }

        static float? FindArc(SurfaceProfile profile, System.Func<SurfaceSample, bool> predicate)
        {
            for (int i = 0; i < profile.SampleCount; i++)
                if (predicate(profile.SampleAtIndex(i)))
                    return (i + 0.5f) * profile.Spacing;
            return null;
        }

        [Test]
        public void BandAtDepth_UsesDefaults()
        {
            var (layer, _, _) = Build();
            Assert.AreEqual(0, layer.BandAtDepth(0.5f));
            Assert.AreEqual(1, layer.BandAtDepth(2f));
            Assert.AreEqual(-1, layer.BandAtDepth(10f));
        }

        [Test]
        public void UnderWater_TopBandIsWater()
        {
            var (layer, profile, _) = Build();
            var arc = FindArc(profile, s => s.Terrain == TerrainKind.Water);
            Assert.IsTrue(arc.HasValue);
            Assert.AreEqual(UndergroundMaterial.Water, layer.MaterialAt(0, arc.Value));
        }

        [TestCase("Snow", UndergroundMaterial.Ice)]
        [TestCase("Desert", UndergroundMaterial.Sand)]
        [TestCase("Grassland", UndergroundMaterial.Soil)]
        public void TopBand_FollowsBiome(string biomeId, UndergroundMaterial expected)
        {
            for (int seed = 1; seed <= 10; seed++)
            {
                var (layer, profile, biomes) = Build(seed);
                var arc = FindArc(profile, s => biomes[s.Biome].Id == biomeId);
                if (!arc.HasValue)
                    continue;
                Assert.AreEqual(expected, layer.MaterialAt(0, arc.Value));
                return;
            }
            Assert.Inconclusive($"No {biomeId} on the rim in seeds 1-10.");
        }

        [Test]
        public void DeepestBand_IsRockOrWater()
        {
            var (layer, profile, _) = Build();
            var deepest = layer.Bands.Count - 1;
            for (int i = 0; i < profile.SampleCount; i++)
            {
                var material = layer.MaterialAt(deepest, (i + 0.5f) * profile.Spacing);
                Assert.That(material, Is.EqualTo(UndergroundMaterial.Rock).Or.EqualTo(UndergroundMaterial.Water));
            }
        }

        [Test]
        public void SameSeed_SameMaterials()
        {
            var (a, profile, _) = Build(4);
            var (b, _, _) = Build(4);
            for (int band = 0; band < a.Bands.Count; band++)
                for (int i = 0; i < profile.SampleCount; i++)
                {
                    var arc = (i + 0.5f) * profile.Spacing;
                    Assert.AreEqual(a.MaterialAt(band, arc), b.MaterialAt(band, arc));
                }
        }

        [Test]
        public void EachBand_HasOwnTrack()
        {
            var (layer, _, _) = Build();
            Assert.AreNotSame(layer.Track(0), layer.Track(1));
            Assert.AreEqual(3, Enumerable.Range(0, layer.Bands.Count).Select(layer.Track).Distinct().Count());
        }
    }
}
