using UnityEngine;

namespace PocketPlanet.Planet
{
    public enum RowDepthKind
    {
        Building,
        Flora
    }

    // Rows are drawn by their place relative to the active row: the active row in front, the others behind it
    // in wrap-around order, so stepping through the rows turns them like a carousel.
    // Rows sit behind the planet (order -2), so anything below the rim is hidden, and behind characters (0 and up).
    // Within a row, flora sorts just behind buildings.
    public static class RowDepth
    {
        const float InactiveDim = 0.6f;
        static readonly int[] SortingBases = { -5, -7, -9 };
        static readonly float[] Scales = { 1f, 0.91f, 0.82f };
        static readonly float[] Shades = { 1f, 0.86f * InactiveDim, 0.72f * InactiveDim };

        public static int Place(SurfaceRow row, SurfaceRow active)
        {
            var count = SurfaceRows.FrontToBack.Count;
            return (SurfaceRows.DepthIndex(row) - SurfaceRows.DepthIndex(active) + count) % count;
        }

        public static int Place(SurfaceRow row) => Place(row, BuildRowSelection.ActiveRow ?? SurfaceRow.Front);

        public static int SortingOrder(SurfaceRow row, RowDepthKind kind)
        {
            var rowBase = SortingBases[Place(row)];
            return kind == RowDepthKind.Flora ? rowBase : rowBase + 1;
        }

        public static float Scale(SurfaceRow row) => Scales[Place(row)];

        public static float Shade(SurfaceRow row) => Shades[Place(row)];

        public static Color Tint(SurfaceRow row)
        {
            var shade = Shade(row);
            return new Color(shade, shade, shade, 1f);
        }
    }
}
