using NUnit.Framework;

namespace PocketPlanet.Planet.Tests
{
    public class ZoomScaleTests
    {
        static readonly ZoomScale Scale = new ZoomScale(1f, 32f);

        [Test]
        public void Ends_MapToNearestAndFarthest()
        {
            Assert.AreEqual(32f, Scale.OrthoAt(0.1f), 1e-3f);
            Assert.AreEqual(1f, Scale.OrthoAt(1f), 1e-3f);
        }

        [TestCase(0.1f)]
        [TestCase(0.37f)]
        [TestCase(0.8f)]
        [TestCase(1f)]
        public void LevelAt_InvertsOrthoAt(float level)
        {
            Assert.AreEqual(level, Scale.LevelAt(Scale.OrthoAt(level)), 1e-4f);
        }

        [Test]
        public void EqualLevelSteps_ScaleOrthoByEqualFactors()
        {
            var a = Scale.OrthoAt(0.4f) / Scale.OrthoAt(0.5f);
            var b = Scale.OrthoAt(0.8f) / Scale.OrthoAt(0.9f);
            Assert.AreEqual(a, b, 1e-3f);
        }

        [Test]
        public void LevelAfterScaling_ClampsToRange()
        {
            Assert.AreEqual(1f, Scale.LevelAfterScaling(0.95f, 0.01f), 1e-4f);
            Assert.AreEqual(0.1f, Scale.LevelAfterScaling(0.2f, 100f), 1e-4f);
        }

        [Test]
        public void ShrinkingOrtho_ZoomsIn()
        {
            Assert.Greater(Scale.LevelAfterScaling(0.5f, 0.8f), 0.5f);
        }
    }
}
