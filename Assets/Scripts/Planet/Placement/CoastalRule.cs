namespace PocketPlanet.Planet
{
    public class CoastalRule : IPlacementRule
    {
        public PlacementReason Evaluate(ArcRange range, SurfaceProfile profile, BiomeTable biomes)
        {
            bool hasLand = false, hasWater = false, unbuildable = false;
            foreach (var sample in profile.SamplesIn(range))
            {
                if (sample.Terrain == TerrainKind.Water)
                {
                    hasWater = true;
                    continue;
                }
                hasLand = true;
                unbuildable |= !biomes[sample.Biome].Buildable;
            }

            if (!hasLand || !hasWater)
                return PlacementReason.RuleViolation;
            return unbuildable ? PlacementReason.BiomeNotBuildable : PlacementReason.None;
        }
    }
}
