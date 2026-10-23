using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    [CreateAssetMenu(menuName = "Pocket Planet/Planet Settings")]
    public class PlanetSettings : ScriptableObject
    {
        public int Seed;
        [Min(5f)] public float Circumference = 48f;
        [Min(0.05f)] public float BlockSize = 0.6f;
        [Range(1, 8)] public int ContinentCount = 3;
        [Range(0.2f, 0.9f)] public float LandCoverage = 0.6f;
        [Range(0f, 1f)] public float CoastRoughness = 0.5f;
        [Range(0.05f, 1f)] public float SurfaceSampleSpacing = 0.25f;
        public List<BiomeDefinition> BiomeTable = new List<BiomeDefinition>();

        public List<BandDefinition> UndergroundBands = new List<BandDefinition>
        {
            new BandDefinition("Topsoil", 0f, 1f),
            new BandDefinition("Bedrock", 1f, 3f),
            new BandDefinition("Deep", 3f, 6f)
        };

        public List<BandDefinition> AirBands = new List<BandDefinition>
        {
            new BandDefinition("Low", 0f, 2f),
            new BandDefinition("Flight", 2f, 5f),
            new BandDefinition("High", 5f, 8f),
            new BandDefinition("Orbit", 8f, 14f)
        };

        public float Radius => Circumference / (2f * Mathf.PI);
        public int GridSize => Mathf.CeilToInt(2f * Radius / BlockSize);
        public float SampleSpacing => SurfaceSampleSpacing * BlockSize;
    }
}
