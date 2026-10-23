using System;
using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public class AirLayer
    {
        readonly float circumference;

        public AirLayer(IReadOnlyList<BandDefinition> bands, float circumference)
        {
            Bands = bands;
            this.circumference = circumference;
        }

        public IReadOnlyList<BandDefinition> Bands { get; }
        public AirTraffic Traffic { get; } = new AirTraffic();
        public AirZones Zones { get; } = new AirZones();

        public int BandAt(float height)
        {
            for (int i = 0; i < Bands.Count; i++)
                if (Bands[i].Contains(height))
                    return i;
            return -1;
        }

        public IEnumerable<IAirborne> InBand(int band)
        {
            foreach (var entity in Traffic.All)
                if (BandAt(entity.Position.Height) == band)
                    yield return entity;
        }

        public IEnumerable<IAirborne> InRange(ArcRange range, int? band = null)
        {
            foreach (var entity in Traffic.All)
                if (range.Contains(entity.Position.Arc) && (!band.HasValue || BandAt(entity.Position.Height) == band.Value))
                    yield return entity;
        }

        public IAirborne Nearest(PlanetCoord from, Func<IAirborne, bool> filter = null)
        {
            IAirborne nearest = null;
            var best = float.MaxValue;
            foreach (var entity in Traffic.All)
            {
                if (filter != null && !filter(entity))
                    continue;
                var distance = DistanceBetween(from, entity.Position);
                if (distance < best)
                {
                    best = distance;
                    nearest = entity;
                }
            }
            return nearest;
        }

        public IEnumerable<AirZone> ZonesAt(PlanetCoord coord)
        {
            var band = BandAt(coord.Height);
            foreach (var zone in Zones.All)
                if (zone.Band == band && zone.Range.Contains(coord.Arc))
                    yield return zone;
        }

        float DistanceBetween(PlanetCoord a, PlanetCoord b)
        {
            var arc = ArcRange.Distance(a.Arc, b.Arc, circumference);
            return Mathf.Sqrt(arc * arc + (a.Height - b.Height) * (a.Height - b.Height));
        }
    }
}
