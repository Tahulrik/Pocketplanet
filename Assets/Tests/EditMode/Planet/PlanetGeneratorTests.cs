using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;

namespace PocketPlanet.Planet.Tests
{
    public class PlanetGeneratorTests
    {
        [Test]
        public void Generate_SameSeed_IdenticalLayers()
        {
            var a = TestPlanets.Generate(11);
            var b = TestPlanets.Generate(11);
            for (int x = 0; x < a.BlockMap.Size; x++)
                for (int y = 0; y < a.BlockMap.Size; y++)
                    Assert.AreEqual(a.BlockMap[x, y], b.BlockMap[x, y]);
            for (int i = 0; i < a.Surface.Profile.SampleCount; i++)
            {
                Assert.AreEqual(a.Surface.Profile.SampleAtIndex(i).Biome, b.Surface.Profile.SampleAtIndex(i).Biome);
                var arc = (i + 0.5f) * a.Surface.Profile.Spacing;
                for (int band = 0; band < a.Underground.Bands.Count; band++)
                    Assert.AreEqual(a.Underground.MaterialAt(band, arc), b.Underground.MaterialAt(band, arc));
            }
        }

        [Test]
        public void Radius_FromCircumference() =>
            Assert.AreEqual(48f / (2f * Mathf.PI), TestPlanets.Generate().Frame.Radius, 1e-4);

        [Test]
        public void Frame_UsesGivenCenter()
        {
            var planet = PlanetGenerator.Generate(TestPlanets.Settings(), new Vector2(3f, 4f));
            Assert.AreEqual(new Vector2(3f, 4f), planet.Frame.Center);
        }

        [Test]
        public void Generate_Circumference500_Under100msInEditor()
        {
            var settings = TestPlanets.Settings(circumference: 500f);
            PlanetGenerator.Generate(settings);
            var watch = Stopwatch.StartNew();
            PlanetGenerator.Generate(settings);
            watch.Stop();
            Assert.Less(watch.ElapsedMilliseconds, 100);
        }
    }
}
