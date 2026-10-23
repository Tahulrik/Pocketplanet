using NUnit.Framework;

namespace PocketPlanet.Planet.Tests
{
    public class SeededNoiseTests
    {
        [Test]
        public void SameSeed_SameValues() =>
            Assert.AreEqual(new SeededNoise(7).Fractal(1.3f, 2.1f, 4), new SeededNoise(7).Fractal(1.3f, 2.1f, 4));

        [Test]
        public void DifferentSeed_DifferentValues() =>
            Assert.AreNotEqual(new SeededNoise(7).Value(1.3f, 2.1f), new SeededNoise(8).Value(1.3f, 2.1f));

        [Test]
        public void Range_IsZeroToOne()
        {
            var noise = new SeededNoise(3);
            for (int i = 0; i < 1000; i++)
                Assert.That(noise.Fractal(i * 0.37f, i * 0.11f, 4), Is.InRange(0f, 1f));
        }

        [Test]
        public void Value_IsContinuous() =>
            Assert.AreEqual(new SeededNoise(5).Value(2.5f, 3.5f), new SeededNoise(5).Value(2.501f, 3.5f), 0.02f);
    }
}
