using System.Collections.Generic;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public static class DefaultBiomes
    {
        public static List<BiomeDefinition> Create() => new List<BiomeDefinition>
        {
            Land("Grassland", "#00AE2F", "#3A3A3A", "#2BFF5A", new Vector2(0f, 1f), new Vector2(0f, 1f), "Fertile"),
            Land("Forest", "#007A24", "#262626", "#00D63F", new Vector2(0.25f, 0.65f), new Vector2(0.55f, 1f), "Fertile"),
            Land("Desert", "#E8C766", "#5A4A3A", "#FFE27A", new Vector2(0.65f, 1f), new Vector2(0f, 0.35f), "Arid"),
            Land("Snow", "#F2F6F8", "#6B6B6B", "#FFFFFF", new Vector2(0f, 0.25f), new Vector2(0f, 1f), "Cold"),
            Water(BiomeTable.ShallowWaterId, "#8ECDF7", "#F07A2A", "#A8EEFF"),
            Water(BiomeTable.DeepWaterId, "#63B3F2", "#C4300F", "#6FD3FF")
        };

        static BiomeDefinition Land(string id, string normal, string doom, string hallowed,
            Vector2 temperature, Vector2 moisture, string tag)
        {
            var biome = Create(id, BiomeKind.Land, normal, doom, hallowed, tag);
            biome.TemperatureRange = temperature;
            biome.MoistureRange = moisture;
            return biome;
        }

        static BiomeDefinition Water(string id, string normal, string doom, string hallowed)
        {
            var biome = Create(id, BiomeKind.Water, normal, doom, hallowed, "Water");
            biome.Buildable = false;
            return biome;
        }

        static BiomeDefinition Create(string id, BiomeKind kind, string normal, string doom, string hallowed, string tag)
        {
            var biome = ScriptableObject.CreateInstance<BiomeDefinition>();
            biome.name = id;
            biome.Id = id;
            biome.DisplayName = id;
            biome.Kind = kind;
            biome.NormalColor = Parse(normal);
            biome.DoomColor = Parse(doom);
            biome.HallowedColor = Parse(hallowed);
            biome.Tags = new[] { tag };
            return biome;
        }

        static Color Parse(string hex) => ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.magenta;
    }
}
