namespace PocketPlanet.Planet
{
    public sealed class PlanetModel : IPlanet
    {
        public PlanetModel(PlanetSettings settings, PlanetFrame frame, BiomeTable biomes, BlockMap blockMap,
            SurfaceLayer surface, UndergroundLayer underground, AirLayer air, PlanetQueries queries, PlacementValidator placement)
        {
            Placement = placement;
            Settings = settings;
            Frame = frame;
            Biomes = biomes;
            BlockMap = blockMap;
            Surface = surface;
            Underground = underground;
            Air = air;
            Queries = queries;
        }

        public PlanetSettings Settings { get; }
        public PlanetFrame Frame { get; }
        public BiomeTable Biomes { get; }
        public BlockMap BlockMap { get; }
        public SurfaceLayer Surface { get; }
        public UndergroundLayer Underground { get; }
        public AirLayer Air { get; }
        public PlanetQueries Queries { get; }
        public PlacementValidator Placement { get; }
    }
}
