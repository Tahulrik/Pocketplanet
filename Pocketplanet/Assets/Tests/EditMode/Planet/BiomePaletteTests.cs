using NUnit.Framework;
using UnityEngine;

namespace PocketPlanet.Planet.Tests
{
    public class BiomePaletteTests
    {
        static BiomeDefinition Grassland() => DefaultBiomes.Create()[0];

        static void AssertColor(Color expected, Color actual, float tolerance = 1e-4f)
        {
            Assert.AreEqual(expected.r, actual.r, tolerance);
            Assert.AreEqual(expected.g, actual.g, tolerance);
            Assert.AreEqual(expected.b, actual.b, tolerance);
        }

        [Test]
        public void Evaluate_MinusOne_IsDoom() => AssertColor(Grassland().DoomColor, BiomePalette.Evaluate(Grassland(), -1f));

        [Test]
        public void Evaluate_Zero_IsNormal() => AssertColor(Grassland().NormalColor, BiomePalette.Evaluate(Grassland(), 0f));

        [Test]
        public void Evaluate_PlusOne_IsHallowed() => AssertColor(Grassland().HallowedColor, BiomePalette.Evaluate(Grassland(), 1f));

        [Test]
        public void Evaluate_Half_IsMidpointToHallowed()
        {
            var biome = Grassland();
            AssertColor(Color.Lerp(biome.NormalColor, biome.HallowedColor, 0.5f), BiomePalette.Evaluate(biome, 0.5f));
        }

        [Test]
        public void Evaluate_ClampsOutOfRange() => AssertColor(BiomePalette.Evaluate(Grassland(), 1f), BiomePalette.Evaluate(Grassland(), 3f));

        [Test]
        public void BuildTexture_RowsMatchColors()
        {
            var biomes = DefaultBiomes.Create();
            var texture = BiomePalette.BuildTexture(new BiomeTable(biomes));
            Assert.AreEqual(biomes.Count, texture.width);
            Assert.AreEqual(3, texture.height);
            for (int i = 0; i < biomes.Count; i++)
            {
                AssertColor(biomes[i].DoomColor, texture.GetPixel(i, 0), 1f / 255f);
                AssertColor(biomes[i].NormalColor, texture.GetPixel(i, 1), 1f / 255f);
                AssertColor(biomes[i].HallowedColor, texture.GetPixel(i, 2), 1f / 255f);
            }
        }

        [Test]
        public void BuildBiomeMap_StoresIndices_EmptyIs255()
        {
            var map = new BlockMap(4, 1f);
            map[1, 2] = 3;
            var texture = BiomePalette.BuildBiomeMap(map);
            Assert.AreEqual(FilterMode.Point, texture.filterMode);
            Assert.AreEqual(255, Mathf.RoundToInt(texture.GetPixel(0, 0).r * 255f));
            Assert.AreEqual(3, Mathf.RoundToInt(texture.GetPixel(1, 2).r * 255f));
        }
    }
}
