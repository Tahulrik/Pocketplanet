namespace PocketPlanet.Planet
{
    public class LandOnlyRule : IPlacementRule
    {
        public PlacementReason Evaluate(ArcRange range, SurfaceProfile profile, BiomeTable biomes)
        {
            var unbuildable = false;
            foreach (var sample in profile.SamplesIn(range))
            {
                if (sample.Terrain == TerrainKind.Water)
                    return PlacementReason.Water;
                unbuildable |= !biomes[sample.Biome].Buildable;
            }
            return unbuildable ? PlacementReason.BiomeNotBuildable : PlacementReason.None;
        }
    }
}
