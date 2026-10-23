using NUnit.Framework;
using UnityEngine;

namespace PocketPlanet.Planet.Tests
{
    public class PlanetFrameTests
    {
        [Test]
        public void RoundTrip()
        {
            var frame = new PlanetFrame(Vector2.zero, 10f);
            var world = frame.ToWorld(new PlanetCoord(5f, 2f));
            var coord = frame.ToCoord(world);
            Assert.AreEqual(5f, coord.Arc, 1e-3);
            Assert.AreEqual(2f, coord.Height, 1e-3);
        }

        [Test]
        public void ArcZero_IsPlusX()
        {
            var world = new PlanetFrame(Vector2.zero, 10f).ToWorld(new PlanetCoord(0f, 0f));
            Assert.AreEqual(10f, world.x, 1e-4);
            Assert.AreEqual(0f, world.y, 1e-4);
        }

        [Test]
        public void ToCoord_AtCenter_IsFinite()
        {
            var coord = new PlanetFrame(Vector2.zero, 10f).ToCoord(Vector2.zero);
            Assert.IsFalse(float.IsNaN(coord.Arc));
            Assert.AreEqual(0f, coord.Arc);
            Assert.AreEqual(-10f, coord.Height, 1e-4);
        }

        [Test]
        public void ToCoord_FarAway_StillMapsToArc()
        {
            var frame = new PlanetFrame(new Vector2(3, 3), 10f);
            Assert.AreEqual(frame.Circumference / 4f, frame.ToCoord(new Vector2(3, 503)).Arc, 1e-2);
        }

        [Test]
        public void UprightRotation_PointsOutward()
        {
            var frame = new PlanetFrame(Vector2.zero, 10f);
            Vector2 up = frame.UprightRotation(frame.Circumference / 4f) * Vector3.up;
            Assert.AreEqual(0f, up.x, 1e-4);
            Assert.AreEqual(1f, up.y, 1e-4);
        }
    }
}
