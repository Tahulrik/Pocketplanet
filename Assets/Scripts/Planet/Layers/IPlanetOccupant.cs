using System.Collections.Generic;

namespace PocketPlanet.Planet
{
    public enum OccupantKind
    {
        Building,
        ConstructionSite,
        Prop
    }

    public interface IPlanetOccupant
    {
        string Id { get; }
        OccupantKind Kind { get; }
        IReadOnlyCollection<string> Tags { get; }
    }
}
