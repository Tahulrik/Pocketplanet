namespace PocketPlanet.Planet
{
    public interface IPlanet
    {
        PlanetSettings Settings { get; }
        PlanetFrame Frame { get; }
        BiomeTable Biomes { get; }
        BlockMap BlockMap { get; }
        SurfaceLayer Surface { get; }
        UndergroundLayer Underground { get; }
        AirLayer Air { get; }
        PlanetQueries Queries { get; }
    }
}
