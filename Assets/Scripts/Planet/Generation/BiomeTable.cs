using System;
using System.Collections.Generic;

namespace PocketPlanet.Planet
{
    public class BiomeTable
    {
        public const string ShallowWaterId = "ShallowWater";
        public const string DeepWaterId = "DeepWater";
        public const int MaxBiomes = 254;

        readonly List<BiomeDefinition> biomes;
        readonly List<byte> landIndices = new List<byte>();

        public BiomeTable(IReadOnlyList<BiomeDefinition> biomes)
        {
            if (biomes == null || biomes.Count == 0)
                throw new ArgumentException("Biome table is empty.");
            if (biomes.Count > MaxBiomes)
                throw new ArgumentException($"Biome table supports at most {MaxBiomes} biomes.");

            this.biomes = new List<BiomeDefinition>(biomes);
            for (byte i = 0; i < this.biomes.Count; i++)
                if (this.biomes[i].Kind == BiomeKind.Land)
                    landIndices.Add(i);

            if (landIndices.Count == 0)
                throw new ArgumentException("Biome table needs at least one land biome.");
            ShallowWater = IndexOfRequired(ShallowWaterId);
            DeepWater = IndexOfRequired(DeepWaterId);
        }

        public int Count => biomes.Count;
        public BiomeDefinition this[byte index] => biomes[index];
        public byte ShallowWater { get; }
        public byte DeepWater { get; }

        public bool IsLand(byte index) => index < biomes.Count && biomes[index].Kind == BiomeKind.Land;

        public byte ClassifyLand(float temperature, float moisture)
        {
            for (int i = 1; i < landIndices.Count; i++)
                if (biomes[landIndices[i]].Matches(temperature, moisture))
                    return landIndices[i];
            return landIndices[0];
        }

        byte IndexOfRequired(string id)
        {
            var index = biomes.FindIndex(b => b.Id == id);
            if (index < 0)
                throw new ArgumentException($"Biome table is missing required biome '{id}'.");
            return (byte)index;
        }
    }
}
