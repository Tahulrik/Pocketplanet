using System;
using System.Collections.Generic;

namespace PocketPlanet.Planet
{
    public class AirZones
    {
        readonly List<AirZone> zones = new List<AirZone>();

        public event Action<AirZone> ZoneAdded;
        public event Action<AirZone> ZoneRemoved;

        public IReadOnlyList<AirZone> All => zones;

        public void Add(AirZone zone)
        {
            zones.Add(zone);
            ZoneAdded?.Invoke(zone);
        }

        public bool Remove(AirZone zone)
        {
            if (!zones.Remove(zone))
                return false;
            ZoneRemoved?.Invoke(zone);
            return true;
        }

        public IEnumerable<AirZone> ZonesIn(ArcRange range, int band)
        {
            foreach (var zone in zones)
                if (zone.Band == band && zone.Range.Overlaps(range))
                    yield return zone;
        }
    }
}
