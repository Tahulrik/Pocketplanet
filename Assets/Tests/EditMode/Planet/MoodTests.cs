using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace PocketPlanet.Planet.Tests
{
    public class MoodTests
    {
        static void AssertColor(Color expected, Color actual)
        {
            Assert.AreEqual(expected.r, actual.r, 1e-4);
            Assert.AreEqual(expected.g, actual.g, 1e-4);
            Assert.AreEqual(expected.b, actual.b, 1e-4);
        }

        [Test]
        public void Smoother_MovesTowardsTarget_AtRate() =>
            Assert.AreEqual(0.125f, MoodSmoother.Step(0f, 1f, 0.5f, 0.25f), 1e-5);

        [Test]
        public void Smoother_DoesNotOvershoot() =>
            Assert.AreEqual(0.3f, MoodSmoother.Step(0.25f, 0.3f, 1f, 1f), 1e-5);

        [Test]
        public void Smoother_Clamps() =>
            Assert.AreEqual(1f, MoodSmoother.Step(0.9f, 5f, 10f, 1f), 1e-5);

        [Test]
        public void Grading_Endpoints()
        {
            Assert.AreEqual(-40f, MoodGrading.Evaluate(-1f).Saturation, 1e-4);
            Assert.AreEqual(0f, MoodGrading.Evaluate(0f).Saturation, 1e-4);
            Assert.AreEqual(35f, MoodGrading.Evaluate(1f).Saturation, 1e-4);
            AssertColor(new Color(1f, 0.72f, 0.6f), MoodGrading.Evaluate(-1f).Filter);
            AssertColor(Color.white, MoodGrading.Evaluate(0f).Filter);
            AssertColor(new Color(1f, 0.97f, 0.88f), MoodGrading.Evaluate(1f).Filter);
        }

        [Test]
        public void Grading_IsMonotonic()
        {
            var previous = float.MinValue;
            for (int i = -10; i <= 10; i++)
            {
                var saturation = MoodGrading.Evaluate(i / 10f).Saturation;
                Assert.Greater(saturation, previous);
                previous = saturation;
            }
        }

        [Test]
        public void Controller_RaisesMoodChanged_AndAppliesEffects()
        {
            var go = new GameObject("mood");
            try
            {
                var source = go.AddComponent<ManualMoodSource>();
                var effect = new RecordingEffect();
                var controller = go.AddComponent<WorldMoodController>();
                controller.Configure(source, 1f);
                controller.Register(effect);
                source.SetMood(-1f);

                var changes = new List<float>();
                controller.MoodChanged += changes.Add;
                controller.Tick(0.5f);
                controller.Tick(0.5f);
                controller.Tick(0.5f);

                CollectionAssert.AreEqual(new[] { -0.5f, -1f }, changes);
                Assert.AreEqual(-1f, effect.Applied[effect.Applied.Count - 1], 1e-5);
                Assert.AreEqual(-1f, controller.CurrentMood, 1e-5);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
