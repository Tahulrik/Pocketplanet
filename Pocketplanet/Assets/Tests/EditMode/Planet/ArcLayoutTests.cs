using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace PocketPlanet.Planet.Tests
{
    public class ArcLayoutTests
    {
        const float Slot = 30f;

        [Test]
        public void Capacity_GroundAt30Degrees_Is5()
        {
            Assert.AreEqual(5, ArcLayout.Capacity(ArcShape.Ground, Slot));
            Assert.AreEqual(12, ArcLayout.Capacity(ArcShape.Ring, Slot));
        }

        [Test]
        public void Fitting_ThreeActions_CentredLeftToRight()
        {
            var slots = ArcLayout.Layout(ArcShape.Ground, Slot, 3, 0);
            AssertActions(slots, (0, -30f), (1, 0f), (2, 30f));
            Assert.IsFalse(slots.Any(s => s.Kind != ArcSlotKind.Action));
        }

        [Test]
        public void Fitting_TwoActions_StraddleTop() =>
            AssertActions(ArcLayout.Layout(ArcShape.Ground, Slot, 2, 0), (0, -15f), (1, 15f));

        [Test]
        public void NoActions_NoSlots()
        {
            Assert.IsEmpty(ArcLayout.Layout(ArcShape.Ground, Slot, 0, 0));
            Assert.AreEqual(1, ArcLayout.PageCount(ArcShape.Ground, Slot, 0));
        }

        [Test]
        public void GroundOverflow_FirstPage_HasOnlyNextArrowOnRight()
        {
            var slots = ArcLayout.Layout(ArcShape.Ground, Slot, 7, 0);
            AssertActions(slots, (0, -60f), (1, -30f), (2, 0f), (3, 30f));
            AssertArrows(slots, null, 60f);
        }

        [Test]
        public void GroundOverflow_LastPage_HasOnlyPreviousArrowOnLeft()
        {
            var slots = ArcLayout.Layout(ArcShape.Ground, Slot, 7, 1);
            AssertActions(slots, (4, -30f), (5, 0f), (6, 30f));
            AssertArrows(slots, -60f, null);
        }

        [Test]
        public void GroundOverflow_MiddlePage_HasBothArrows()
        {
            Assert.AreEqual(3, ArcLayout.PageCount(ArcShape.Ground, Slot, 10));
            var slots = ArcLayout.Layout(ArcShape.Ground, Slot, 10, 1);
            AssertActions(slots, (4, -30f), (5, 0f), (6, 30f));
            AssertArrows(slots, -60f, 60f);
        }

        [Test]
        public void Ground_PageIsClamped()
        {
            Assert.AreEqual(1, ArcLayout.NormalizePage(ArcShape.Ground, Slot, 7, 5));
            Assert.AreEqual(0, ArcLayout.NormalizePage(ArcShape.Ground, Slot, 7, -2));
        }

        [Test]
        public void RingOverflow_ArrowsFlankBottom()
        {
            var slots = ArcLayout.Layout(ArcShape.Ring, Slot, 15, 0);
            AssertArrows(slots, 195f, 165f);
            Assert.AreEqual(10, slots.Count(s => s.Kind == ArcSlotKind.Action));
        }

        [Test]
        public void Ring_PagesWrap()
        {
            Assert.AreEqual(0, ArcLayout.NormalizePage(ArcShape.Ring, Slot, 15, 2));
            Assert.AreEqual(1, ArcLayout.NormalizePage(ArcShape.Ring, Slot, 15, -1));
        }

        [Test]
        public void EveryActionAppearsExactlyOnce()
        {
            foreach (ArcShape shape in Enum.GetValues(typeof(ArcShape)))
                for (int count = 0; count <= 20; count++)
                {
                    var shown = new List<int>();
                    for (int page = 0; page < ArcLayout.PageCount(shape, Slot, count); page++)
                        shown.AddRange(ArcLayout.Layout(shape, Slot, count, page)
                            .Where(s => s.Kind == ArcSlotKind.Action).Select(s => s.ActionIndex));
                    CollectionAssert.AreEqual(Enumerable.Range(0, count), shown.OrderBy(i => i), $"{shape} with {count} actions");
                    Assert.AreEqual(count, shown.Count, $"{shape} with {count} actions");
                }
        }

        [Test]
        public void GroundTooNarrow_Throws() =>
            Assert.Throws<ArgumentException>(() => ArcLayout.Layout(ArcShape.Ground, 60f, 9, 0));

        static void AssertActions(IReadOnlyList<ArcSlot> slots, params (int index, float angle)[] expected)
        {
            var actions = slots.Where(s => s.Kind == ArcSlotKind.Action).ToList();
            Assert.AreEqual(expected.Length, actions.Count);
            foreach (var (index, angle) in expected)
            {
                var slot = actions.Single(s => s.ActionIndex == index);
                AssertAngle(angle, slot.Angle);
            }
        }

        static void AssertArrows(IReadOnlyList<ArcSlot> slots, float? previous, float? next)
        {
            AssertArrow(slots, ArcSlotKind.PreviousPage, previous);
            AssertArrow(slots, ArcSlotKind.NextPage, next);
        }

        static void AssertArrow(IReadOnlyList<ArcSlot> slots, ArcSlotKind kind, float? angle)
        {
            var arrows = slots.Where(s => s.Kind == kind).ToList();
            if (angle == null)
            {
                Assert.IsEmpty(arrows, kind.ToString());
                return;
            }
            Assert.AreEqual(1, arrows.Count, kind.ToString());
            Assert.AreEqual(-1, arrows[0].ActionIndex);
            AssertAngle(angle.Value, arrows[0].Angle);
        }

        static void AssertAngle(float expected, float actual) =>
            Assert.AreEqual(0f, Mathf.DeltaAngle(expected, actual), 1e-3, $"expected {expected}, got {actual}");
    }
}
