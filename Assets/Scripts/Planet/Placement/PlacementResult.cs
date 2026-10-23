namespace PocketPlanet.Planet
{
    public readonly struct PlacementResult
    {
        public PlacementResult(PlacementReason reason, SurfaceRow row, ArcRange range)
        {
            Reason = reason;
            Row = row;
            Range = range;
        }

        public PlacementReason Reason { get; }
        public SurfaceRow Row { get; }
        public ArcRange Range { get; }
        public bool IsValid => Reason == PlacementReason.None;
    }
}
