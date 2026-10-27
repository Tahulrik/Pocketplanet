using System.Collections.Generic;

namespace PocketPlanet.Planet
{
    public enum SurfaceRow
    {
        Back,
        Middle,
        Front
    }

    public static class SurfaceRows
    {
        public static readonly IReadOnlyList<SurfaceRow> FrontToBack = new[] { SurfaceRow.Front, SurfaceRow.Middle, SurfaceRow.Back };
    }

    public class SurfaceLayer
    {
        readonly Dictionary<SurfaceRow, OccupancyTrack> rows = new Dictionary<SurfaceRow, OccupancyTrack>();

        public SurfaceLayer(SurfaceProfile profile)
        {
            Profile = profile;
            foreach (var row in SurfaceRows.FrontToBack)
                rows[row] = new OccupancyTrack(profile.Circumference);
        }

        public SurfaceProfile Profile { get; }

        public OccupancyTrack Row(SurfaceRow row) => rows[row];

        public bool IsFreeInAllRows(ArcRange range)
        {
            foreach (var track in rows.Values)
                if (!track.IsFree(range))
                    return false;
            return true;
        }
    }
}
