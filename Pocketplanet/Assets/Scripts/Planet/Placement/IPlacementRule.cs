namespace PocketPlanet.Planet
{
    public interface IPlacementRule
    {
        PlacementReason Evaluate(ArcRange range, SurfaceProfile profile, BiomeTable biomes);
    }
}
