using UnityEngine;

namespace PocketPlanet.Planet
{
    public class PlanetFrame
    {
        public Vector2 Center { get; set; }
        public float Radius { get; }
        public float Circumference => 2f * Mathf.PI * Radius;

        public PlanetFrame(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public PlanetCoord ToCoord(Vector2 world)
        {
            var offset = world - Center;
            var angle = offset.sqrMagnitude > 0f ? Mathf.Atan2(offset.y, offset.x) : 0f;
            return new PlanetCoord(ArcRange.Wrap(angle * Radius, Circumference), offset.magnitude - Radius);
        }

        public Vector2 ToWorld(PlanetCoord coord) => Center + Outward(coord.Arc) * (Radius + coord.Height);

        public Vector2 Outward(float arc)
        {
            var angle = arc / Radius;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        public Quaternion UprightRotation(float arc) => Quaternion.LookRotation(Vector3.forward, Outward(arc));
    }
}
