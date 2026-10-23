using NUnit.Framework;

namespace PocketPlanet.Planet.Tests
{
    public class ArcRangeTests
    {
        const float C = 48f;

        [Test]
        public void Wrap_NegativeAndOverflow()
        {
            Assert.AreEqual(47f, ArcRange.Wrap(-1f, C), 1e-4);
            Assert.AreEqual(1f, ArcRange.Wrap(49f, C), 1e-4);
        }

        [Test]
        public void Distance_IsShortestWay() => Assert.AreEqual(2f, ArcRange.Distance(47f, 1f, C), 1e-4);

        [Test]
        public void SignedDelta_AcrossSeam() => Assert.AreEqual(2f, ArcRange.SignedDelta(47f, 1f, C), 1e-4);

        [Test]
        public void SignedDelta_Backwards_IsNegative() => Assert.AreEqual(-2f, ArcRange.SignedDelta(1f, 47f, C), 1e-4);

        [Test]
        public void Centered_AcrossSeam_ContainsBothSides()
        {
            var r = ArcRange.Centered(0.1f, 2f, C);
            Assert.IsTrue(r.Contains(47.5f));
            Assert.IsTrue(r.Contains(0.9f));
            Assert.IsFalse(r.Contains(2f));
            Assert.AreEqual(0.1f, r.Center, 1e-4);
        }

        [Test]
        public void Overlaps_AcrossSeam()
        {
            Assert.IsTrue(ArcRange.Centered(0f, 2f, C).Overlaps(ArcRange.Centered(47.5f, 1f, C)));
            Assert.IsFalse(ArcRange.Centered(0f, 2f, C).Overlaps(ArcRange.Centered(3f, 1f, C)));
        }

        [Test]
        public void Overlaps_TouchingEdgesDoNotOverlap() =>
            Assert.IsFalse(new ArcRange(0f, 2f, C).Overlaps(new ArcRange(2f, 2f, C)));

        [Test]
        public void ZeroLength_ContainsNothing() => Assert.IsFalse(new ArcRange(5f, 0f, C).Contains(5f));

        [Test]
        public void FullLength_ContainsEverything() => Assert.IsTrue(new ArcRange(5f, 100f, C).Contains(4.9f));
    }
}
