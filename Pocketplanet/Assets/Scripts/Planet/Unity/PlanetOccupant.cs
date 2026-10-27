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

        public bool SettleOnNearestValidSpot(Footprint footprint)
        {
            var planet = PlanetHost.Active.Planet;
            var placement = planet.Placement.FindAnywhere(footprint, planet.Frame.ToCoord(transform.position).Arc);
            return placement.HasValue && Settle(placement.Value);
        }

        public bool Settle(PlacementResult placement)
        {
            MoveTo(placement);
            return Reserve(placement);
        }

        public void MoveTo(PlacementResult placement)
        {
            var frame = PlanetHost.Active.Planet.Frame;
            var arc = placement.Range.Center;
            var surface = frame.ToWorld(new PlanetCoord(arc, 0f));
            transform.SetPositionAndRotation(
                new Vector3(surface.x, surface.y, transform.position.z), frame.UprightRotation(arc));
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
