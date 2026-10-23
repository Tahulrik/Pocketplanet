using System.Collections.Generic;

namespace PocketPlanet.Planet
{
    public enum AirZoneKind
    {
        Storm,
        AshCloud,
        NoFly
    }

    public sealed class AirZone
    {
        public AirZone(ArcRange range, int band, AirZoneKind kind, IReadOnlyCollection<string> tags)
        {
            Range = range;
            Band = band;
            Kind = kind;
            Tags = tags;
        }

        public ArcRange Range { get; }
        public int Band { get; }
        public AirZoneKind Kind { get; }
        public IReadOnlyCollection<string> Tags { get; }
    }
}
