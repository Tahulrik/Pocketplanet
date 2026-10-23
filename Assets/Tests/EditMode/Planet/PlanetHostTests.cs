using NUnit.Framework;
using UnityEngine;

namespace PocketPlanet.Planet.Tests
{
    public class PlanetHostTests
    {
        GameObject go;

        [SetUp]
        public void SetUp() => go = new GameObject("planet");

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(go);

        PlanetHost NewHost()
        {
            var host = go.AddComponent<PlanetHost>();
            host.Configure(TestPlanets.Settings(), null);
            return host;
        }

        [Test]
        public void Host_Generates_OnFirstAccess_AndSizesCollider()
        {
            var host = NewHost();
            Assert.IsNotNull(host.Planet);
            Assert.AreEqual(host.Planet.Frame.Radius, go.GetComponent<CircleCollider2D>().radius, 1e-4);
        }

        [Test]
        public void Host_Active_IsSetAndCleared()
        {
            var host = NewHost();
            Assert.AreSame(host, PlanetHost.Active);
            host.enabled = false;
            Assert.IsNull(PlanetHost.Active);
        }

        [Test]
        public void Host_FrameFollowsTransform()
        {
            var host = NewHost();
            var planet = host.Planet;
            go.transform.position = new Vector3(2f, 3f, 0f);
            host.SyncFrame();
            Assert.AreEqual(new Vector2(2f, 3f), planet.Frame.Center);
        }

        [Test]
        public void Regenerate_RaisesGenerated_WithNewPlanet()
        {
            var host = NewHost();
            IPlanet generated = null;
            host.Generated += p => generated = p;
            var planet = host.Regenerate();
            Assert.AreSame(planet, generated);
            Assert.AreSame(planet, host.Planet);
        }

        [Test]
        public void PendingRegeneration_IgnoredWhilePlaying()
        {
            var host = NewHost();
            var planet = host.Planet;
            host.RequestRegeneration();
            host.ProcessPendingRegeneration(isPlaying: true);
            Assert.AreSame(planet, host.Planet);
        }

        [Test]
        public void PendingRegeneration_AppliedInEditMode()
        {
            var host = NewHost();
            var planet = host.Planet;
            host.RequestRegeneration();
            host.ProcessPendingRegeneration(isPlaying: false);
            Assert.AreNotSame(planet, host.Planet);
        }

        [Test]
        public void Regenerate_ClearsPendingRequest()
        {
            var host = NewHost();
            host.RequestRegeneration();
            var planet = host.Regenerate();
            host.ProcessPendingRegeneration(isPlaying: false);
            Assert.AreSame(planet, host.Planet);
        }

        [Test]
        public void Host_WithoutSettings_Throws()
        {
            var host = go.AddComponent<PlanetHost>();
            Assert.Throws<System.InvalidOperationException>(() => host.Regenerate());
        }
    }
}
