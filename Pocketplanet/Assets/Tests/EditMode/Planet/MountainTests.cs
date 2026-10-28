using System.Linq;
using NUnit.Framework;

namespace PocketPlanet.Planet.Tests
{
    public class MountainTests
    {
        [Test]
        public void Generation_PlacesMountains()
        {
            Assert.IsNotEmpty(TestPlanets.GenerateWithMountains(2).Surface.Mountains);
        }

        [Test]
        public void NoMountains_WhenRateIsZero()
        {
            Assert.IsEmpty(TestPlanets.Generate(2).Surface.Mountains);
        }

        [Test]
        public void Mountains_StandFullyOnLand()
        {
            var planet = TestPlanets.GenerateWithMountains(3);
            foreach (var mountain in planet.Surface.Mountains)
                Assert.IsTrue(planet.Surface.Profile.IsAll(mountain.Range, s => s.Terrain == TerrainKind.Land), mountain.Id);
        }

        [Test]
        public void Mountains_ReserveTheirRow_AndBlockBuildingThere()
        {
            var planet = TestPlanets.GenerateWithMountains(4);
            var mountain = planet.Surface.Mountains.First();
            Assert.IsFalse(planet.Surface.Row(mountain.Row).IsFree(mountain.Range));
            var result = planet.Placement.Check(new Footprint(0.5f, RowPreference.Any, new LandOnlyRule()), mountain.Range.Center, mountain.Row);
            Assert.AreEqual(PlacementReason.Occupied, result.Reason);
        }

        [Test]
        public void Mountains_InTheSameRow_NeverOverlap()
        {
            var planet = TestPlanets.GenerateWithMountains(5, 0.3f);
            var mountains = planet.Surface.Mountains;
            for (int i = 0; i < mountains.Count; i++)
                for (int j = i + 1; j < mountains.Count; j++)
                    if (mountains[i].Row == mountains[j].Row)
                        Assert.IsFalse(mountains[i].Range.Overlaps(mountains[j].Range));
        }

        [Test]
        public void SameSeed_SameMountains()
        {
            var a = TestPlanets.GenerateWithMountains(6).Surface.Mountains.Select(m => m.Range.Center).ToArray();
            var b = TestPlanets.GenerateWithMountains(6).Surface.Mountains.Select(m => m.Range.Center).ToArray();
            CollectionAssert.AreEqual(a, b);
        }

        [Test]
        public void HeightAt_FlatPeak_SlopesToZeroAtTheFeet()
        {
            var mountain = new MountainFeature(0, new ArcRange(10f, 4f, 48f), SurfaceRow.Back, 2f, 0.3f, 0);
            Assert.AreEqual(2f, mountain.HeightAt(12f), 1e-4f);
            Assert.AreEqual(2f, mountain.HeightAt(12.5f), 1e-4f);
            Assert.AreEqual(0f, mountain.HeightAt(10f), 1e-4f);
            Assert.AreEqual(0f, mountain.HeightAt(20f), 1e-4f);
            var slope = mountain.HeightAt(11f);
            Assert.That(slope, Is.GreaterThan(0f).And.LessThan(2f));
            Assert.IsTrue(mountain.Peak.Contains(12f));
            Assert.IsFalse(mountain.Peak.Contains(10.5f));
        }

        [Test]
        public void Mountain_IsATaggedOccupant()
        {
            var mountain = TestPlanets.GenerateWithMountains(2).Surface.Mountains.First();
            Assert.AreEqual(OccupantKind.Mountain, mountain.Kind);
            CollectionAssert.Contains(mountain.Tags, MountainFeature.Tag);
        }
    }
}
