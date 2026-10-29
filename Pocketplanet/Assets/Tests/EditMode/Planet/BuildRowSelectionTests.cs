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
        public void PushBack_MovesEveryRowDeeper_AndWrapsTheBackRowToTheFront()
        {
            selection.PushBack();

            Assert.AreEqual(SurfaceRow.Back, selection.Current);
            Assert.AreEqual(0, RowDepth.Place(SurfaceRow.Back));
            Assert.AreEqual(1, RowDepth.Place(SurfaceRow.Front));
            Assert.AreEqual(2, RowDepth.Place(SurfaceRow.Middle));
        }

        [Test]
        public void PushForward_BringsTheNextRowToTheFront_AndSendsTheFrontRowToTheBack()
        {
            selection.PushForward();

            Assert.AreEqual(SurfaceRow.Middle, selection.Current);
            Assert.AreEqual(1, RowDepth.Place(SurfaceRow.Back));
            Assert.AreEqual(2, RowDepth.Place(SurfaceRow.Front));
        }

        [Test]
        public void ThreePushes_ComeFullCircle()
        {
            selection.PushBack();
            selection.PushBack();
            selection.PushBack();
            Assert.AreEqual(SurfaceRow.Front, selection.Current);
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
        public void RowsFurtherBackOnTheBelt_SitDeeperInTheShade()
        {
            selection.Select(SurfaceRow.Middle);

            Assert.AreEqual(1f, RowDepth.Shade(SurfaceRow.Middle));
            Assert.Less(RowDepth.Shade(SurfaceRow.Back), 1f);
            Assert.Less(RowDepth.Shade(SurfaceRow.Front), RowDepth.Shade(SurfaceRow.Back));
        }

        [Test]
        public void PushingBack_RestylesRows_SoTheActiveOneIsInFront()
        {
            var front = StyledObject(SurfaceRow.Front);
            var back = StyledObject(SurfaceRow.Back);
            try
            {
                selection.PushBack();

                Assert.Greater(Order(back), Order(front));
                Assert.Greater(back.transform.localScale.x, front.transform.localScale.x);
                Assert.AreEqual(Color.white, back.GetComponentInChildren<SpriteRenderer>().color);
                Assert.Less(front.GetComponentInChildren<SpriteRenderer>().color.r, 1f);
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
