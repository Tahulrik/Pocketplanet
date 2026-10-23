using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PocketPlanet.Planet.Tests
{
    public class FakeOccupant : IPlanetOccupant
    {
        public FakeOccupant(string id) => Id = id;
        public string Id { get; }
        public OccupantKind Kind => OccupantKind.Building;
        public IReadOnlyCollection<string> Tags { get; } = new string[0];
    }

    public class OccupancyTrackTests
    {
        const float C = 48f;
        readonly FakeOccupant a = new FakeOccupant("a");
        readonly FakeOccupant b = new FakeOccupant("b");

        [Test]
        public void TryAdd_Overlap_Rejected_TouchingAllowed()
        {
            var track = new OccupancyTrack(C);
            Assert.IsTrue(track.TryAdd(a, new ArcRange(10f, 2f, C)));
            Assert.IsFalse(track.TryAdd(b, new ArcRange(11f, 2f, C)));
            Assert.IsTrue(track.TryAdd(b, new ArcRange(12f, 2f, C)));
        }

        [Test]
        public void TryAdd_AcrossSeam_Rejected()
        {
            var track = new OccupancyTrack(C);
            track.TryAdd(a, ArcRange.Centered(0.1f, 2f, C));
            Assert.IsFalse(track.TryAdd(b, ArcRange.Centered(47.5f, 1f, C)));
        }

        [Test]
        public void Remove_FreesRange_AndRaisesEvent()
        {
            var track = new OccupancyTrack(C);
            var range = new ArcRange(10f, 2f, C);
            track.TryAdd(a, range);
            var removed = 0;
            track.OccupantRemoved += (o, r) => removed++;
            Assert.IsTrue(track.Remove(a));
            Assert.AreEqual(1, removed);
            Assert.IsTrue(track.IsFree(range));
        }

        [Test]
        public void TryAdd_RaisesAddedEvent()
        {
            var track = new OccupancyTrack(C);
            IPlanetOccupant added = null;
            track.OccupantAdded += (o, r) => added = o;
            track.TryAdd(a, new ArcRange(1f, 1f, C));
            Assert.AreSame(a, added);
        }

        [Test]
        public void Move_IgnoresOwnRange()
        {
            var track = new OccupancyTrack(C);
            track.TryAdd(a, new ArcRange(10f, 2f, C));
            Assert.IsTrue(track.Move(a, new ArcRange(11f, 2f, C)));
            Assert.IsTrue(track.TryGetRange(a, out var range));
            Assert.AreEqual(11f, range.Start, 1e-4);
        }

        [Test]
        public void Move_IntoOther_FailsAndKeepsOldRange()
        {
            var track = new OccupancyTrack(C);
            track.TryAdd(a, new ArcRange(10f, 2f, C));
            track.TryAdd(b, new ArcRange(20f, 2f, C));
            Assert.IsFalse(track.Move(a, new ArcRange(19f, 2f, C)));
            track.TryGetRange(a, out var range);
            Assert.AreEqual(10f, range.Start, 1e-4);
        }

        [Test]
        public void IsFree_IgnoresGivenOccupant()
        {
            var track = new OccupancyTrack(C);
            track.TryAdd(a, new ArcRange(10f, 2f, C));
            Assert.IsFalse(track.IsFree(new ArcRange(10f, 1f, C)));
            Assert.IsTrue(track.IsFree(new ArcRange(10f, 1f, C), a));
        }

        [Test]
        public void OccupantsIn_ReturnsOnlyOverlapping()
        {
            var track = new OccupancyTrack(C);
            track.TryAdd(a, new ArcRange(10f, 2f, C));
            track.TryAdd(b, new ArcRange(20f, 2f, C));
            var found = track.OccupantsIn(new ArcRange(9f, 4f, C)).Select(o => o.Occupant).ToList();
            CollectionAssert.AreEqual(new[] { a }, found);
        }

        [Test]
        public void DuplicateAdd_Rejected()
        {
            var track = new OccupancyTrack(C);
            track.TryAdd(a, new ArcRange(10f, 2f, C));
            Assert.IsFalse(track.TryAdd(a, new ArcRange(30f, 2f, C)));
        }

        [Test]
        public void SurfaceLayer_HasIndependentRows()
        {
            var layer = new SurfaceLayer(TestPlanets.Profile(TestPlanets.Settings()));
            var range = new ArcRange(5f, 2f, layer.Profile.Circumference);
            layer.Row(SurfaceRow.Front).TryAdd(a, range);
            Assert.IsTrue(layer.Row(SurfaceRow.Back).IsFree(range));
        }
    }
}
