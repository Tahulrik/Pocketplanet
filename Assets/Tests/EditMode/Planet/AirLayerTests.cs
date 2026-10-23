using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace PocketPlanet.Planet.Tests
{
    public class FakeAirborne : IAirborne
    {
        public FakeAirborne(string id, float arc, float height, params string[] tags)
        {
            Id = id;
            Position = new PlanetCoord(arc, height);
            Tags = tags;
        }

        public string Id { get; }
        public PlanetCoord Position { get; set; }
        public Vector2 Velocity => Vector2.zero;
        public IReadOnlyCollection<string> Tags { get; }
    }

    public class AirLayerTests
    {
        const float C = 48f;

        static AirLayer NewLayer() => new AirLayer(TestPlanets.Settings().AirBands, C);

        [Test]
        public void BandAt_Defaults()
        {
            var air = NewLayer();
            Assert.AreEqual(0, air.BandAt(1f));
            Assert.AreEqual(1, air.BandAt(3f));
            Assert.AreEqual(-1, air.BandAt(20f));
            Assert.AreEqual(-1, air.BandAt(-1f));
        }

        [Test]
        public void InBand_FiltersByHeight()
        {
            var air = NewLayer();
            var low = new FakeAirborne("low", 5f, 1f);
            var plane = new FakeAirborne("plane", 5f, 3f);
            air.Traffic.Register(low);
            air.Traffic.Register(plane);
            CollectionAssert.AreEqual(new[] { plane }, air.InBand(1).ToList());
        }

        [Test]
        public void InRange_AcrossSeam()
        {
            var air = NewLayer();
            var a = new FakeAirborne("a", 47.8f, 3f);
            var b = new FakeAirborne("b", 20f, 3f);
            air.Traffic.Register(a);
            air.Traffic.Register(b);
            CollectionAssert.AreEqual(new[] { a }, air.InRange(ArcRange.Centered(0f, 1f, C)).ToList());
        }

        [Test]
        public void InRange_WithBand_Filters()
        {
            var air = NewLayer();
            air.Traffic.Register(new FakeAirborne("low", 5f, 1f));
            Assert.IsEmpty(air.InRange(ArcRange.Centered(5f, 2f, C), 1));
        }

        [Test]
        public void Nearest_UsesWrappedArc()
        {
            var air = NewLayer();
            var near = new FakeAirborne("near", 0.5f, 3f);
            var far = new FakeAirborne("far", 45f, 3f);
            air.Traffic.Register(far);
            air.Traffic.Register(near);
            Assert.AreSame(near, air.Nearest(new PlanetCoord(47.5f, 3f)));
        }

        [Test]
        public void Nearest_RespectsFilter()
        {
            var air = NewLayer();
            var plane = new FakeAirborne("plane", 10f, 3f, "Plane");
            air.Traffic.Register(new FakeAirborne("bird", 5f, 1f, "Bird"));
            air.Traffic.Register(plane);
            Assert.AreSame(plane, air.Nearest(new PlanetCoord(5f, 1f), a => a.Tags.Contains("Plane")));
            Assert.IsNull(air.Nearest(new PlanetCoord(5f, 1f), a => a.Tags.Contains("Ufo")));
        }

        [Test]
        public void ZonesAt_MatchesBandAndRange()
        {
            var air = NewLayer();
            air.Zones.Add(new AirZone(new ArcRange(10f, 4f, C), 1, AirZoneKind.Storm, new string[0]));
            Assert.AreEqual(1, air.ZonesAt(new PlanetCoord(11f, 3f)).Count());
            Assert.IsEmpty(air.ZonesAt(new PlanetCoord(11f, 1f)));
            Assert.IsEmpty(air.ZonesAt(new PlanetCoord(20f, 3f)));
        }

        [Test]
        public void ZonesIn_RangeAndBand()
        {
            var air = NewLayer();
            var storm = new AirZone(new ArcRange(10f, 4f, C), 1, AirZoneKind.Storm, new string[0]);
            air.Zones.Add(storm);
            CollectionAssert.AreEqual(new[] { storm }, air.Zones.ZonesIn(new ArcRange(13f, 5f, C), 1).ToList());
            Assert.IsTrue(air.Zones.Remove(storm));
            Assert.IsEmpty(air.Zones.ZonesIn(new ArcRange(13f, 5f, C), 1));
        }

        [Test]
        public void Register_Twice_ReturnsFalse()
        {
            var air = NewLayer();
            var a = new FakeAirborne("a", 1f, 1f);
            Assert.IsTrue(air.Traffic.Register(a));
            Assert.IsFalse(air.Traffic.Register(a));
        }

        [Test]
        public void Events_Fire()
        {
            var air = NewLayer();
            int registered = 0, unregistered = 0, added = 0, removed = 0;
            air.Traffic.Registered += _ => registered++;
            air.Traffic.Unregistered += _ => unregistered++;
            air.Zones.ZoneAdded += _ => added++;
            air.Zones.ZoneRemoved += _ => removed++;
            var a = new FakeAirborne("a", 1f, 1f);
            var zone = new AirZone(new ArcRange(1f, 1f, C), 0, AirZoneKind.NoFly, new string[0]);
            air.Traffic.Register(a);
            air.Traffic.Unregister(a);
            air.Zones.Add(zone);
            air.Zones.Remove(zone);
            Assert.AreEqual(new[] { 1, 1, 1, 1 }, new[] { registered, unregistered, added, removed });
        }
    }
}
