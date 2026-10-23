using System;
using System.Collections.Generic;

namespace PocketPlanet.Planet
{
    public class OccupancyTrack
    {
        readonly float circumference;
        readonly Dictionary<IPlanetOccupant, ArcRange> ranges = new Dictionary<IPlanetOccupant, ArcRange>();

        public OccupancyTrack(float circumference)
        {
            this.circumference = circumference;
        }

        public event Action<IPlanetOccupant, ArcRange> OccupantAdded;
        public event Action<IPlanetOccupant, ArcRange> OccupantRemoved;

        public int Count => ranges.Count;

        public bool TryAdd(IPlanetOccupant occupant, ArcRange range)
        {
            if (ranges.ContainsKey(occupant) || !IsFree(range))
                return false;
            ranges.Add(occupant, Normalize(range));
            OccupantAdded?.Invoke(occupant, range);
            return true;
        }

        public bool Remove(IPlanetOccupant occupant)
        {
            if (!ranges.TryGetValue(occupant, out var range))
                return false;
            ranges.Remove(occupant);
            OccupantRemoved?.Invoke(occupant, range);
            return true;
        }

        public bool Move(IPlanetOccupant occupant, ArcRange newRange)
        {
            if (!ranges.ContainsKey(occupant) || !IsFree(newRange, occupant))
                return false;
            ranges[occupant] = Normalize(newRange);
            return true;
        }

        public bool IsFree(ArcRange range, IPlanetOccupant ignore = null)
        {
            foreach (var pair in ranges)
                if (pair.Key != ignore && pair.Value.Overlaps(range))
                    return false;
            return true;
        }

        public IEnumerable<(IPlanetOccupant Occupant, ArcRange Range)> OccupantsIn(ArcRange range)
        {
            foreach (var pair in ranges)
                if (pair.Value.Overlaps(range))
                    yield return (pair.Key, pair.Value);
        }

        public bool TryGetRange(IPlanetOccupant occupant, out ArcRange range) => ranges.TryGetValue(occupant, out range);

        ArcRange Normalize(ArcRange range) => new ArcRange(range.Start, range.Length, circumference);
    }
}
