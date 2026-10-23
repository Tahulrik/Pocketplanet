using NUnit.Framework;

namespace PocketPlanet.Planet.Tests
{
    public class PlanetScaleTests
    {
        [Test]
        public void FromReference_SameRadius_IsUnchanged() =>
            Assert.AreEqual(33f, PlanetScale.FromReference(33f, 8f, 8f), 1e-4);

        [Test]
        public void FromReference_DoubleRadius_DoublesValue() =>
            Assert.AreEqual(66f, PlanetScale.FromReference(33f, 8f, 16f), 1e-4);

        [Test]
        public void FromReference_DefaultPlanet_ShrinksSlightly() =>
            Assert.AreEqual(33f * 7.639f / 8f, PlanetScale.FromReference(33f, 8f, 48f / (2f * UnityEngine.Mathf.PI)), 1e-2);
    }
}
