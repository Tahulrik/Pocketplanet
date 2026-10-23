using UnityEngine;

namespace PocketPlanet.Planet
{
    public static class PlanetGenerator
    {
        public static PlanetModel Generate(PlanetSettings settings, Vector2 center = default)
        {
            var biomes = new BiomeTable(settings.BiomeTable);
            var rng = new System.Random(settings.Seed);
            var landArcs = RimLayout.BuildLandArcs(settings, rng);
            var blockMap = BlockMapGenerator.Generate(settings, biomes, landArcs, settings.Seed);
            var profile = SurfaceProfile.Build(blockMap, biomes, settings.Radius, settings.SampleSpacing);
            var surface = new SurfaceLayer(profile);
            var underground = UndergroundLayer.Build(profile, biomes, settings.UndergroundBands, settings.Seed + 1);
            var air = new AirLayer(settings.AirBands, profile.Circumference);
            var queries = new PlanetQueries(surface, biomes);
            var frame = new PlanetFrame(center, settings.Radius);
            var placement = new PlacementValidator(surface, biomes);
            return new PlanetModel(settings, frame, biomes, blockMap, surface, underground, air, queries, placement);
        }
    }
}
