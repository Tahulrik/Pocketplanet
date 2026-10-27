using NUnit.Framework;
using UnityEngine;

namespace PocketPlanet.Planet.Tests
{
    public class BuildRowSelectionTests
    {
        BuildRowSelection selection;

        [SetUp]
        public void SetUp() => selection = new GameObject("rows").AddComponent<BuildRowSelection>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(selection.gameObject);

        [Test]
        public void StepBack_GoesOneLayerDeeper_AndStopsAtBack()
        {
            selection.StepBack();
            Assert.AreEqual(SurfaceRow.Middle, selection.Current);
            selection.StepBack();
            selection.StepBack();
            Assert.AreEqual(SurfaceRow.Back, selection.Current);
        }

        [Test]
        public void StepForward_StopsAtFront()
        {
            selection.Select(SurfaceRow.Middle);
            selection.StepForward();
            selection.StepForward();
            Assert.AreEqual(SurfaceRow.Front, selection.Current);
        }

        [Test]
        public void CycleBackward_WrapsFromBackToFront()
        {
            selection.Select(SurfaceRow.Back);
            selection.CycleBackward();
            Assert.AreEqual(SurfaceRow.Front, selection.Current);
        }

        [Test]
        public void IsInFrontOf_FollowsDepthOrder()
        {
            Assert.IsTrue(SurfaceRows.IsInFrontOf(SurfaceRow.Front, SurfaceRow.Middle));
            Assert.IsTrue(SurfaceRows.IsInFrontOf(SurfaceRow.Middle, SurfaceRow.Back));
            Assert.IsFalse(SurfaceRows.IsInFrontOf(SurfaceRow.Back, SurfaceRow.Front));
            Assert.IsFalse(SurfaceRows.IsInFrontOf(SurfaceRow.Middle, SurfaceRow.Middle));
        }
    }
}
