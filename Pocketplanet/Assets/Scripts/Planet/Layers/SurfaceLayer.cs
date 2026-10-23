namespace PocketPlanet.Planet
{
    public enum SurfaceRow
    {
        Back,
        Front
    }

    public class SurfaceLayer
    {
        readonly OccupancyTrack back;
        readonly OccupancyTrack front;

        public SurfaceLayer(SurfaceProfile profile)
        {
            Profile = profile;
            back = new OccupancyTrack(profile.Circumference);
            front = new OccupancyTrack(profile.Circumference);
        }

        public SurfaceProfile Profile { get; }

        public OccupancyTrack Row(SurfaceRow row) => row == SurfaceRow.Back ? back : front;
    }
}
