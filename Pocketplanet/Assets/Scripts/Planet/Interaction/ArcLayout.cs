using System;
using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public enum ArcShape { Ground, Ring }

    public enum ArcSlotKind { Action, PreviousPage, NextPage }

    // Angle is in degrees from the object's local up, clockwise.
    public readonly struct ArcSlot
    {
        public ArcSlot(float angle, ArcSlotKind kind, int actionIndex = -1)
        {
            Angle = angle;
            Kind = kind;
            ActionIndex = actionIndex;
        }

        public float Angle { get; }
        public ArcSlotKind Kind { get; }
        public int ActionIndex { get; }
    }

    // Places menu buttons on an arch (Ground) or a full circle (Ring), paging with arrow slots when they don't fit.
    public static class ArcLayout
    {
        public const float GroundSpan = 160f;
        const float RingSpan = 360f;
        const int MinGroundPagingCapacity = 3;
        const int RingArrowCount = 2;

        public static float SpanOf(ArcShape shape) => shape == ArcShape.Ground ? GroundSpan : RingSpan;

        public static int Capacity(ArcShape shape, float slotAngle) => Mathf.FloorToInt(SpanOf(shape) / slotAngle + 1e-4f);

        public static int PageCount(ArcShape shape, float slotAngle, int actionCount)
        {
            var capacity = Capacity(shape, slotAngle);
            if (actionCount <= capacity)
                return 1;
            if (shape == ArcShape.Ring)
                return CeilDiv(actionCount, capacity - RingArrowCount);

            RequirePagingRoom(capacity);
            var afterFirstPage = actionCount - (capacity - 1);
            if (afterFirstPage <= capacity - 1)
                return 2;
            return 2 + CeilDiv(afterFirstPage - (capacity - 1), capacity - 2);
        }

        public static int NormalizePage(ArcShape shape, float slotAngle, int actionCount, int page)
        {
            var pages = PageCount(shape, slotAngle, actionCount);
            return shape == ArcShape.Ring ? (page % pages + pages) % pages : Mathf.Clamp(page, 0, pages - 1);
        }

        public static IReadOnlyList<ArcSlot> Layout(ArcShape shape, float slotAngle, int actionCount, int page)
        {
            var capacity = Capacity(shape, slotAngle);
            if (actionCount <= capacity)
                return Centred(shape, slotAngle, actionCount);
            page = NormalizePage(shape, slotAngle, actionCount, page);
            return shape == ArcShape.Ring
                ? RingPage(capacity, actionCount, page)
                : GroundPage(slotAngle, capacity, actionCount, page);
        }

        static List<ArcSlot> Centred(ArcShape shape, float slotAngle, int count)
        {
            var slots = new List<ArcSlot>(count);
            for (int i = 0; i < count; i++)
            {
                var angle = (i - (count - 1) / 2f) * slotAngle;
                slots.Add(new ArcSlot(shape == ArcShape.Ring ? Wrap(angle) : angle, ArcSlotKind.Action, i));
            }
            return slots;
        }

        // The first page has no back arrow and the last no forward arrow, so they hold one more action than middle pages.
        static List<ArcSlot> GroundPage(float slotAngle, int capacity, int actionCount, int page)
        {
            RequirePagingRoom(capacity);
            var pages = PageCount(ArcShape.Ground, slotAngle, actionCount);
            var hasPrevious = page > 0;
            var hasNext = page < pages - 1;
            var firstAction = page == 0 ? 0 : (capacity - 1) + (page - 1) * (capacity - 2);
            var slots = new List<ArcSlot>(capacity);

            float AngleOf(int slot) => (slot - (capacity - 1) / 2f) * slotAngle;

            if (hasPrevious)
                slots.Add(new ArcSlot(AngleOf(0), ArcSlotKind.PreviousPage));
            var firstSlot = hasPrevious ? 1 : 0;
            var lastSlot = hasNext ? capacity - 2 : capacity - 1;
            for (int slot = firstSlot, action = firstAction; slot <= lastSlot && action < actionCount; slot++, action++)
                slots.Add(new ArcSlot(AngleOf(slot), ArcSlotKind.Action, action));
            if (hasNext)
                slots.Add(new ArcSlot(AngleOf(capacity - 1), ArcSlotKind.NextPage));
            return slots;
        }

        // Spreads the slots evenly round the whole circle, with the two arrows flanking the bottom.
        static List<ArcSlot> RingPage(int capacity, int actionCount, int page)
        {
            var spacing = RingSpan / capacity;
            var perPage = capacity - RingArrowCount;
            var slots = new List<ArcSlot>(capacity);

            float AngleOf(int slot) => Wrap(180f + spacing / 2f + slot * spacing);

            slots.Add(new ArcSlot(AngleOf(0), ArcSlotKind.PreviousPage));
            var firstAction = page * perPage;
            for (int i = 0; i < perPage && firstAction + i < actionCount; i++)
                slots.Add(new ArcSlot(AngleOf(1 + i), ArcSlotKind.Action, firstAction + i));
            slots.Add(new ArcSlot(AngleOf(capacity - 1), ArcSlotKind.NextPage));
            return slots;
        }

        static void RequirePagingRoom(int capacity)
        {
            if (capacity < MinGroundPagingCapacity)
                throw new ArgumentException($"A ground arc needs room for at least {MinGroundPagingCapacity} slots to page, but only fits {capacity}.");
        }

        static int CeilDiv(int value, int divisor) => (value + divisor - 1) / divisor;

        static float Wrap(float degrees) => (degrees % RingSpan + RingSpan) % RingSpan;
    }
}
