using UnityEngine;

namespace PocketPlanet.Planet
{
    public enum RowDepthKind
    {
        Building,
        Flora
    }

    // Everything sits between the planet (order -10) and citizens (order 0 and up).
    // Within a row, flora sorts just behind buildings.
    public static class RowDepth
    {
        public static int SortingOrder(SurfaceRow row, RowDepthKind kind)
        {
            var rowBase = row == SurfaceRow.Back ? -9 : row == SurfaceRow.Middle ? -7 : -5;
            return kind == RowDepthKind.Flora ? rowBase : rowBase + 1;
        }

        public static float Scale(SurfaceRow row) =>
            row == SurfaceRow.Back ? 0.82f : row == SurfaceRow.Middle ? 0.91f : 1f;

        public static float Shade(SurfaceRow row) =>
            row == SurfaceRow.Back ? 0.72f : row == SurfaceRow.Middle ? 0.86f : 1f;

        public static Color Tint(SurfaceRow row)
        {
            var shade = Shade(row);
            return new Color(shade, shade, shade, 1f);
        }
    }
}
