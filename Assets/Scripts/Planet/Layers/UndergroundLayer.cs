using System.Collections.Generic;

namespace PocketPlanet.Planet
{
    public class UndergroundLayer
    {
        const float PocketNoiseScale = 0.3f;
        const float SoilPocketThreshold = 0.6f;
        const float AquiferNoiseScale = 0.2f;
        const float AquiferThreshold = 0.75f;

        readonly SurfaceProfile profile;
        readonly UndergroundMaterial[,] materials;
        readonly OccupancyTrack[] tracks;

        UndergroundLayer(SurfaceProfile profile, IReadOnlyList<BandDefinition> bands, UndergroundMaterial[,] materials)
        {
            this.profile = profile;
            this.materials = materials;
            Bands = bands;
            tracks = new OccupancyTrack[bands.Count];
            for (int i = 0; i < tracks.Length; i++)
                tracks[i] = new OccupancyTrack(profile.Circumference);
        }

        public static UndergroundLayer Build(SurfaceProfile profile, BiomeTable biomes, IReadOnlyList<BandDefinition> bands, int seed)
        {
            var noise = new SeededNoise(seed);
            var materials = new UndergroundMaterial[bands.Count, profile.SampleCount];
            for (int band = 0; band < bands.Count; band++)
                for (int i = 0; i < profile.SampleCount; i++)
                    materials[band, i] = MaterialFor(band, bands.Count, i * profile.Spacing, profile.SampleAtIndex(i), biomes, noise);
            return new UndergroundLayer(profile, bands, materials);
        }

        public IReadOnlyList<BandDefinition> Bands { get; }

        public int BandAtDepth(float depth)
        {
            for (int i = 0; i < Bands.Count; i++)
                if (Bands[i].Contains(depth))
                    return i;
            return -1;
        }

        public UndergroundMaterial MaterialAt(int band, float arc) => materials[band, profile.IndexOf(arc)];

        public OccupancyTrack Track(int band) => tracks[band];

        static UndergroundMaterial MaterialFor(int band, int bandCount, float arc, SurfaceSample surface,
            BiomeTable biomes, SeededNoise noise)
        {
            if (band == 0)
                return TopMaterial(surface, biomes);
            if (band == bandCount - 1)
                return noise.Fractal(arc * AquiferNoiseScale + 50f, band, 3) > AquiferThreshold
                    ? UndergroundMaterial.Water
                    : UndergroundMaterial.Rock;
            return noise.Fractal(arc * PocketNoiseScale, band, 3) > SoilPocketThreshold
                ? UndergroundMaterial.Soil
                : UndergroundMaterial.Rock;
        }

        static UndergroundMaterial TopMaterial(SurfaceSample surface, BiomeTable biomes)
        {
            if (surface.Terrain == TerrainKind.Water)
                return UndergroundMaterial.Water;
            switch (biomes[surface.Biome].Id)
            {
                case "Snow": return UndergroundMaterial.Ice;
                case "Desert": return UndergroundMaterial.Sand;
                default: return UndergroundMaterial.Soil;
            }
        }
    }
}
