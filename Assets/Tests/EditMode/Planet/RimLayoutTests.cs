using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketPlanet.Planet.Tests
{
    public class RimLayoutTests
    {
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(8)]
        public void ArcCount_EqualsContinentCount(int continents)
        {
            var settings = TestPlanets.Settings(continents: continents, circumference: 120f);
            Assert.AreEqual(continents, RimLayout.BuildLandArcs(settings, new System.Random(1)).Count);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void LandLength_MatchesCoverage(int seed)
        {
            var settings = TestPlanets.Settings(seed);
            var total = RimLayout.BuildLandArcs(settings, new System.Random(seed)).Sum(a => a.Length);
            Assert.AreEqual(0.6f * settings.Circumference, total, 1e-2);
        }

        [Test]
        public void ArcsDoNotOverlap_AndGapsAtLeastThreeBlocks()
        {
            var settings = TestPlanets.Settings(continents: 5);
            var arcs = RimLayout.BuildLandArcs(settings, new System.Random(4));
            for (int i = 0; i < arcs.Count; i++)
            {
                var next = arcs[(i + 1) % arcs.Count];
                var gap = ArcRange.Wrap(next.Start - (arcs[i].Start + arcs[i].Length), settings.Circumference);
                Assert.GreaterOrEqual(gap, 3f * settings.BlockSize - 1e-3);
                Assert.GreaterOrEqual(arcs[i].Length, 3f * settings.BlockSize - 1e-3);
            }
        }

        [Test]
        public void Arcs_AreSortedByStart()
        {
            var arcs = RimLayout.BuildLandArcs(TestPlanets.Settings(continents: 6, circumference: 120f), new System.Random(9));
            for (int i = 1; i < arcs.Count; i++)
                Assert.Less(arcs[i - 1].Start, arcs[i].Start);
        }

        [Test]
        public void TinyPlanet_ClampsContinentCount()
        {
            var settings = TestPlanets.Settings(circumference: 10f, continents: 8);
            LogAssert.Expect(LogType.Warning, new Regex("ContinentCount"));
            var arcs = RimLayout.BuildLandArcs(settings, new System.Random(1));
            Assert.That(arcs.Count, Is.InRange(1, 2));
        }
    }
}
