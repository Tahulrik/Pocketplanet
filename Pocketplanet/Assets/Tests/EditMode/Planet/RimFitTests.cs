using NUnit.Framework;
using UnityEngine;

namespace PocketPlanet.Planet.Tests
{
    public class RimFitTests
    {
        [Test]
        public void Sag_IsZeroForZeroWidth()
        {
            Assert.AreEqual(0f, RimFit.Sag(0f, 7.6f), 1e-5f);
        }

        [Test]
        public void Sag_MatchesCircleGeometry()
        {
            // Half-width 3 on radius 5 sits on a 3-4-5 triangle: the chord is 4 from the centre, so the sag is 1.
            Assert.AreEqual(1f, RimFit.Sag(6f, 5f), 1e-4f);
        }

        [Test]
        public void Sag_GrowsWithWidth()
        {
            Assert.Less(RimFit.Sag(1f, 7.6f), RimFit.Sag(4f, 7.6f));
        }

        [Test]
        public void SinkDepth_IsAtLeastTheSag()
        {
            Assert.Greater(RimFit.SinkDepth(4f, 7.6f), RimFit.Sag(4f, 7.6f));
        }

        [Test]
        public void Foundation_FillsUnderTheBuilding_BehindItsSprites()
        {
            var building = new GameObject("building");
            try
            {
                var wall = new GameObject("wall").AddComponent<SpriteRenderer>();
                wall.transform.SetParent(building.transform, false);
                wall.sortingOrder = -3;

                BuildingFoundation.Ensure(building, 2f, 7.6f);
                BuildingFoundation.Ensure(building, 2f, 7.6f);

                var foundations = 0;
                foreach (Transform child in building.transform)
                    if (child.name == BuildingFoundation.ChildName)
                        foundations++;
                Assert.AreEqual(1, foundations);

                var foundation = building.transform.Find(BuildingFoundation.ChildName).GetComponent<SpriteRenderer>();
                Assert.Less(foundation.sortingOrder, wall.sortingOrder);
                Assert.AreEqual(2f, foundation.bounds.size.x, 1e-3f);
                Assert.Less(foundation.bounds.min.y, -RimFit.Sag(2f, 7.6f));
                Assert.Greater(foundation.bounds.max.y, 0f);
            }
            finally
            {
                Object.DestroyImmediate(building);
            }
        }
    }
}
