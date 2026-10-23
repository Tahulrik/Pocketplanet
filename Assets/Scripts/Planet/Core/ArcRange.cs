using UnityEngine;

namespace PocketPlanet.Planet
{
    public readonly struct ArcRange
    {
        public float Start { get; }
        public float Length { get; }
        public float Circumference { get; }

        public ArcRange(float start, float length, float circumference)
        {
            Circumference = circumference;
            Start = Wrap(start, circumference);
            Length = Mathf.Clamp(length, 0f, circumference);
        }

        public static ArcRange Centered(float center, float width, float circumference) =>
            new ArcRange(center - width / 2f, width, circumference);

        public float End => Wrap(Start + Length, Circumference);
        public float Center => Wrap(Start + Length / 2f, Circumference);

        public bool Contains(float arc) => Wrap(arc - Start, Circumference) < Length;

        public bool Overlaps(ArcRange other) => Contains(other.Start) || other.Contains(Start);

        public static float Wrap(float arc, float circumference)
        {
            var wrapped = arc % circumference;
            if (wrapped < 0f)
                wrapped += circumference;
            return wrapped >= circumference ? 0f : wrapped;
        }

        public static float SignedDelta(float from, float to, float circumference)
        {
            var delta = Wrap(to - from, circumference);
            return delta > circumference / 2f ? delta - circumference : delta;
        }

        public static float Distance(float a, float b, float circumference) =>
            Mathf.Abs(SignedDelta(a, b, circumference));

        public override string ToString() => $"[{Start:0.##} +{Length:0.##}]";
    }
}
