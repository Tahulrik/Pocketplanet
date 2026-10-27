using UnityEngine;

namespace PocketPlanet.Planet
{
    public static class PlanetGizmos
    {
        const float SegmentStep = 0.1f;
        const float FrontRowHeight = 0.2f;
        const float RowHeightStep = 0.15f;
        static readonly Color LandColor = new Color(0.2f, 0.9f, 0.3f);
        static readonly Color WaterColor = new Color(0.3f, 0.6f, 1f);
        static readonly Color FrontColor = new Color(1f, 0.85f, 0.2f);
        static readonly Color MiddleColor = new Color(1f, 0.7f, 0.15f);
        static readonly Color BackColor = new Color(1f, 0.55f, 0.1f);
        static readonly Color BandColor = new Color(1f, 1f, 1f, 0.15f);
        static readonly Color ZoneColor = new Color(1f, 0.2f, 0.2f);

        public static void Draw(IPlanet planet)
        {
            DrawSegments(planet);
            DrawRow(planet, SurfaceRow.Front, FrontRowHeight, FrontColor);
            DrawRow(planet, SurfaceRow.Middle, FrontRowHeight + RowHeightStep, MiddleColor);
            DrawRow(planet, SurfaceRow.Back, FrontRowHeight + 2f * RowHeightStep, BackColor);
            DrawAirBands(planet);
            DrawAirZones(planet);
        }

        public static void DrawArc(PlanetFrame frame, ArcRange range, float height, Color color)
        {
            Gizmos.color = color;
            var steps = Mathf.Max(1, Mathf.CeilToInt(range.Length / SegmentStep));
            var previous = frame.ToWorld(new PlanetCoord(range.Start, height));
            for (int i = 1; i <= steps; i++)
            {
                var next = frame.ToWorld(new PlanetCoord(range.Start + range.Length * i / steps, height));
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }

        static void DrawSegments(IPlanet planet)
        {
            foreach (var segment in planet.Surface.Profile.Segments)
                DrawArc(planet.Frame, segment.Range, 0.05f, segment.Terrain == TerrainKind.Land ? LandColor : WaterColor);
        }

        static void DrawRow(IPlanet planet, SurfaceRow row, float height, Color color)
        {
            var whole = new ArcRange(0f, planet.Frame.Circumference, planet.Frame.Circumference);
            foreach (var (_, range) in planet.Surface.Row(row).OccupantsIn(whole))
                DrawArc(planet.Frame, range, height, color);
        }

        static void DrawAirBands(IPlanet planet)
        {
            var whole = new ArcRange(0f, planet.Frame.Circumference, planet.Frame.Circumference);
            foreach (var band in planet.Air.Bands)
                DrawArc(planet.Frame, whole, band.Max, BandColor);
        }

        static void DrawAirZones(IPlanet planet)
        {
            foreach (var zone in planet.Air.Zones.All)
            {
                var band = planet.Air.Bands[zone.Band];
                DrawArc(planet.Frame, zone.Range, (band.Min + band.Max) / 2f, ZoneColor);
            }
        }
    }
}
