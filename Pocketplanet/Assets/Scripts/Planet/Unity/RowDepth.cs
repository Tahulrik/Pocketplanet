using UnityEngine;

namespace PocketPlanet.Planet
{
    public enum RowDepthKind
    {
        Building,
        Flora
    }

    // Rows keep a fixed depth: Back is always furthest away. Rows sit behind the planet (order -2), so anything
    // below the rim is hidden, and behind characters (0 and up). Within a row, flora sorts just behind buildings.
    // The active row is drawn at full strength, rows behind it are dimmed and rows in front of it turn see-through.
    public static class RowDepth
    {
        const float BehindDim = 0.6f;
        const float InFrontAlpha = 0.3f;

        public static int SortingOrder(SurfaceRow row, RowDepthKind kind)
        {
            var rowBase = row == SurfaceRow.Back ? -9 : row == SurfaceRow.Middle ? -7 : -5;
            return kind == RowDepthKind.Flora ? rowBase : rowBase + 1;
        }

        public static float Scale(SurfaceRow row) =>
            row == SurfaceRow.Back ? 0.82f : row == SurfaceRow.Middle ? 0.91f : 1f;

        public static float Shade(SurfaceRow row) =>
            row == SurfaceRow.Back ? 0.72f : row == SurfaceRow.Middle ? 0.86f : 1f;

        public static Color Tint(SurfaceRow row) => Tint(row, BuildRowSelection.ActiveRow ?? SurfaceRow.Front);

        public static Color Tint(SurfaceRow row, SurfaceRow active)
        {
            if (row == active)
                return Color.white;
            var shade = Shade(row);
            if (SurfaceRows.IsInFrontOf(row, active))
                return new Color(shade, shade, shade, InFrontAlpha);
            shade *= BehindDim;
            return new Color(shade, shade, shade, 1f);
        }
    }
}
