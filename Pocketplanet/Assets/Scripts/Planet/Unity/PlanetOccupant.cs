using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public class PlanetOccupant : MonoBehaviour, IPlanetOccupant
    {
        [SerializeField] OccupantKind kind;
        [SerializeField] string[] tags = new string[0];

        OccupancyTrack track;
        SurfaceRow row;

        public string Id => GetEntityId().ToString();
        public IReadOnlyCollection<string> Tags => tags;

        public OccupantKind Kind
        {
            get => kind;
            set => kind = value;
        }

        public static PlanetOccupant On(GameObject target, OccupantKind kind)
        {
            var occupant = target.GetComponent<PlanetOccupant>();
            if (occupant == null)
                occupant = target.AddComponent<PlanetOccupant>();
            occupant.Kind = kind;
            return occupant;
        }

        public static void ReleaseFrom(GameObject target)
        {
            var occupant = target.GetComponent<PlanetOccupant>();
            if (occupant != null)
                occupant.Release();
        }

        public bool IsReserved => track != null;

        public bool ReserveAtCurrentPosition(Footprint footprint)
        {
            var planet = PlanetHost.Active.Planet;
            var range = planet.Placement.RangeFor(footprint, planet.Frame.ToCoord(transform.position).Arc);
            foreach (var surfaceRow in PlacementValidator.RowsFor(footprint.Row))
                if (Reserve(new PlacementResult(PlacementReason.None, surfaceRow, range)))
                    return true;
            return false;
        }

        public bool Reserve(PlacementResult placement)
        {
            var rowTrack = PlanetHost.Active.Planet.Surface.Row(placement.Row);
            if (!rowTrack.TryAdd(this, placement.Range))
                return false;
            Occupy(rowTrack, placement.Row);
            return true;
        }

        public bool TransferTo(PlanetOccupant next)
        {
            if (track == null || !track.TryGetRange(this, out var range))
                return false;
            var rowTrack = track;
            var currentRow = row;
            Release();
            if (!rowTrack.TryAdd(next, range))
                return false;
            next.Occupy(rowTrack, currentRow);
            return true;
        }

        public void Release()
        {
            track?.Remove(this);
            track = null;
        }

        void Occupy(OccupancyTrack rowTrack, SurfaceRow surfaceRow)
        {
            track = rowTrack;
            row = surfaceRow;
            RowDepthStyle.Apply(gameObject, surfaceRow, RowDepthKind.Building);
        }

        void OnDestroy() => Release();
    }
}
