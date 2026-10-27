using System.Linq;
using NUnit.Framework;

namespace PocketPlanet.Planet.Tests
{
    public class WeatherTests
    {
        [Test]
        public void CloudShape_NoGroupChance_IsSinglePuff()
        {
            var settings = new CloudShapeSettings { GroupChance = 0f };
            var shape = CloudShape.Create(new System.Random(1), settings, 7);
            Assert.AreEqual(1, shape.Puffs.Count);
            Assert.AreEqual(0f, shape.Puffs[0].Offset.x, 1e-4f);
        }

        [Test]
        public void CloudShape_AlwaysGroup_SizeWithinRange()
        {
            var settings = new CloudShapeSettings { GroupChance = 1f, MinGroupSize = 2, MaxGroupSize = 5 };
            var rng = new System.Random(3);
            for (int i = 0; i < 50; i++)
            {
                var count = CloudShape.Create(rng, settings, 7).Puffs.Count;
                Assert.That(count, Is.InRange(2, 5));
            }
        }

        [Test]
        public void CloudShape_Group_IsCenteredAndWiderThanOnePuff()
        {
            var settings = new CloudShapeSettings { GroupChance = 1f, MinGroupSize = 4, MaxGroupSize = 4 };
            var shape = CloudShape.Create(new System.Random(5), settings, 7);
            var halfWidths = shape.Puffs.Select(p => settings.PuffWidth * p.Scale / 2f).ToArray();
            var left = shape.Puffs.Select((p, i) => p.Offset.x - halfWidths[i]).Min();
            var right = shape.Puffs.Select((p, i) => p.Offset.x + halfWidths[i]).Max();
            Assert.AreEqual(0f, (left + right) / 2f, 1e-3f);
            Assert.AreEqual(right - left, shape.Width, 1e-3f);
            Assert.Greater(shape.Width, halfWidths.Max() * 2f);
        }

        [Test]
        public void CloudShape_SpriteIndicesInRange()
        {
            var settings = new CloudShapeSettings { GroupChance = 1f };
            var rng = new System.Random(8);
            for (int i = 0; i < 30; i++)
                Assert.IsTrue(CloudShape.Create(rng, settings, 7).Puffs.All(p => p.SpriteIndex >= 0 && p.SpriteIndex < 7));
        }

        [TestCase(0f, 0f)]
        [TestCase(2.5f, 0.5f)]
        [TestCase(30f, 1f)]
        [TestCase(57.5f, 0.5f)]
        [TestCase(60f, 0f)]
        public void FadeEnvelope_FadesInAndOut(float age, float expected)
        {
            Assert.AreEqual(expected, FadeEnvelope.Alpha(age, 60f, 5f), 1e-4f);
        }

        [Test]
        public void FadeEnvelope_ShortLife_StillPeaksAtOne()
        {
            Assert.AreEqual(1f, FadeEnvelope.Alpha(2f, 4f, 5f), 1e-4f);
        }
    }
}
