using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public interface IAirborne
    {
        string Id { get; }
        PlanetCoord Position { get; }
        Vector2 Velocity { get; }
        IReadOnlyCollection<string> Tags { get; }
    }
}
