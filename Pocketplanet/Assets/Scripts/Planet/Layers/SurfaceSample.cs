namespace PocketPlanet.Planet
{
    public readonly struct SurfaceSample
    {
        public TerrainKind Terrain { get; }
        public byte Biome { get; }
        public bool IsCoast { get; }

        public SurfaceSample(TerrainKind terrain, byte biome, bool isCoast)
        {
            Terrain = terrain;
            Biome = biome;
            IsCoast = isCoast;
        }

        public SurfaceSample WithCoast(bool isCoast) => new SurfaceSample(Terrain, Biome, isCoast);
    }
}
