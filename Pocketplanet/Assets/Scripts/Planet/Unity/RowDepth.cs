using UnityEngine;

namespace PocketPlanet.Planet
{
    public enum RowDepthKind
    {
        Flora,
        Building,
        Scaffold
    }

    // The rows behave like a conveyor belt: the active row is always in front, the others follow behind it
    // in belt order, each one further back sitting deeper in the shade.
    // Rows sit behind the planet (order -2), so anything below the rim is hidden, and behind characters (0 and up).
    // Within a row, flora sorts just behind buildings and scaffolding just in front, so a finished building
    // stays hidden until its scaffold is removed.
    public static class RowDepth
    {
        static readonly int[] SortingBases = { -5, -8, -11 };
        static readonly float[] Scales = { 1f, 0.91f, 0.82f };
        static readonly float[] Shades = { 1f, 0.72f, 0.5f };

        public static int Place(SurfaceRow row, SurfaceRow active)
        {
            var count = SurfaceRows.FrontToBack.Count;
            return (SurfaceRows.DepthIndex(row) - SurfaceRows.DepthIndex(active) + count) % count;
        }

        public static int Place(SurfaceRow row) => Place(row, BuildRowSelection.ActiveRow ?? SurfaceRow.Front);

        public static int SortingOrder(SurfaceRow row, RowDepthKind kind)
        {
            return SortingBases[Place(row)] + (int)kind;
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
