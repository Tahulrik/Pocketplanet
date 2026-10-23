using UnityEngine;

namespace PocketPlanet.Planet
{
    public static class BiomePalette
    {
        public const int DoomRow = 0;
        public const int NormalRow = 1;
        public const int HallowedRow = 2;

        public static Color Evaluate(BiomeDefinition biome, float mood)
        {
            mood = Mathf.Clamp(mood, -1f, 1f);
            return mood < 0f
                ? Color.Lerp(biome.NormalColor, biome.DoomColor, -mood)
                : Color.Lerp(biome.NormalColor, biome.HallowedColor, mood);
        }

        public static Texture2D BuildTexture(BiomeTable biomes)
        {
            var texture = new Texture2D(biomes.Count, 3, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "BiomePalette"
            };
            for (byte i = 0; i < biomes.Count; i++)
            {
                texture.SetPixel(i, DoomRow, biomes[i].DoomColor);
                texture.SetPixel(i, NormalRow, biomes[i].NormalColor);
                texture.SetPixel(i, HallowedRow, biomes[i].HallowedColor);
            }
            texture.Apply();
            return texture;
        }

        public static Texture2D BuildBiomeMap(BlockMap map)
        {
            var texture = new Texture2D(map.Size, map.Size, TextureFormat.R8, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "BiomeMap"
            };
            var data = new byte[map.Size * map.Size];
            for (int y = 0; y < map.Size; y++)
                for (int x = 0; x < map.Size; x++)
                    data[y * map.Size + x] = map[x, y];
            texture.SetPixelData(data, 0);
            texture.Apply();
            return texture;
        }
    }
}
