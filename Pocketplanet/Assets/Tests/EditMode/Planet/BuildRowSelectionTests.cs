using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace PocketPlanet.Planet.Tests
{
    public class BuildRowSelectionTests
    {
        BuildRowSelection selection;

        [SetUp]
        public void SetUp()
        {
            selection = new GameObject("rows").AddComponent<BuildRowSelection>();
            Lifecycle.Call(selection, "OnEnable");
        }

        [TearDown]
        public void TearDown()
        {
            Lifecycle.Call(selection, "OnDisable");
            Object.DestroyImmediate(selection.gameObject);
        }

        [Test]
        public void StepBack_GoesOneLayerDeeper_AndWrapsToFront()
        {
            selection.StepBack();
            Assert.AreEqual(SurfaceRow.Middle, selection.Current);
            selection.StepBack();
            Assert.AreEqual(SurfaceRow.Back, selection.Current);
            selection.StepBack();
            Assert.AreEqual(SurfaceRow.Front, selection.Current);
        }

        [Test]
        public void StepForward_WrapsFromFrontToBack()
        {
            selection.StepForward();
            Assert.AreEqual(SurfaceRow.Back, selection.Current);
            selection.StepForward();
            Assert.AreEqual(SurfaceRow.Middle, selection.Current);
        }

        [Test]
        public void OnlyTheActiveRowIsInteractive()
        {
            selection.Select(SurfaceRow.Middle);
            Assert.IsTrue(BuildRowSelection.IsInteractive(SurfaceRow.Middle));
            Assert.IsFalse(BuildRowSelection.IsInteractive(SurfaceRow.Front));
            Assert.IsFalse(BuildRowSelection.IsInteractive(SurfaceRow.Back));
        }

        [Test]
        public void Place_PutsActiveRowInFront_AndTurnsTheOthersBehindIt()
        {
            Assert.AreEqual(0, RowDepth.Place(SurfaceRow.Middle, SurfaceRow.Middle));
            Assert.AreEqual(1, RowDepth.Place(SurfaceRow.Back, SurfaceRow.Middle));
            Assert.AreEqual(2, RowDepth.Place(SurfaceRow.Front, SurfaceRow.Middle));
            Assert.AreEqual(1, RowDepth.Place(SurfaceRow.Front, SurfaceRow.Back));
        }

        [Test]
        public void SwitchingRows_MovesTheNewRowInFront_AndDimsTheOld()
        {
            var front = StyledObject(SurfaceRow.Front);
            var back = StyledObject(SurfaceRow.Back);
            try
            {
                selection.Select(SurfaceRow.Back);

                Assert.Greater(Order(back), Order(front));
                Assert.Greater(back.GetComponentInChildren<SpriteRenderer>().color.r, front.GetComponentInChildren<SpriteRenderer>().color.r);
                Assert.Greater(back.transform.localScale.x, front.transform.localScale.x);
            }
            finally
            {
                Lifecycle.Call(front.GetComponent<RowDepthStyle>(), "OnDisable");
                Lifecycle.Call(back.GetComponent<RowDepthStyle>(), "OnDisable");
                Object.DestroyImmediate(front);
                Object.DestroyImmediate(back);
            }
        }

        static GameObject StyledObject(SurfaceRow row)
        {
            var target = new GameObject(row.ToString());
            new GameObject("sprite").AddComponent<SpriteRenderer>().transform.SetParent(target.transform, false);
            RowDepthStyle.Apply(target, row, RowDepthKind.Building);
            Lifecycle.Call(target.GetComponent<RowDepthStyle>(), "OnEnable");
            return target;
        }

        static int Order(GameObject target) => target.GetComponent<SortingGroup>().sortingOrder;
    }
}
