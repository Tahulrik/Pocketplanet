using System.Collections.Generic;

namespace PocketPlanet.Planet
{
    // A mountain generated with the planet. It occupies its range in one surface row, so ordinary buildings
    // can't be placed there. Peak and HeightAt describe its shape for buildings that will later sit on it.
    public sealed class MountainFeature : IPlanetOccupant
    {
        public const string Tag = "Mountain";
        static readonly string[] MountainTags = { Tag };

        public MountainFeature(int index, ArcRange range, SurfaceRow row, float height, float peakShare, int variant)
        {
            Id = $"Mountain {index}";
            Range = range;
            Row = row;
            Height = height;
            PeakShare = peakShare;
            Variant = variant;
        }

        public string Id { get; }
        public OccupantKind Kind => OccupantKind.Mountain;
        public IReadOnlyCollection<string> Tags => MountainTags;

        public ArcRange Range { get; }
        public SurfaceRow Row { get; }
        public float Height { get; }
        public float PeakShare { get; }
        public int Variant { get; }

        public ArcRange Peak => ArcRange.Centered(Range.Center, Range.Length * PeakShare, Range.Circumference);

        // Flat on the peak, sloping straight down to the ground at both feet.
        public float HeightAt(float arc)
        {
            var distance = ArcRange.Distance(Range.Center, arc, Range.Circumference);
            var halfWidth = Range.Length / 2f;
            var halfPeak = halfWidth * PeakShare;
            if (distance >= halfWidth)
                return 0f;
            if (distance <= halfPeak)
                return Height;
            return Height * (halfWidth - distance) / (halfWidth - halfPeak);
        }
    }
}
