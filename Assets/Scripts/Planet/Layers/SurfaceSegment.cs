namespace PocketPlanet.Planet
{
    public readonly struct SurfaceSegment
    {
        public ArcRange Range { get; }
        public TerrainKind Terrain { get; }
        public byte Biome { get; }

        public SurfaceSegment(ArcRange range, TerrainKind terrain, byte biome)
        {
            Range = range;
            Terrain = terrain;
            Biome = biome;
        }
    }
}
