using System;
using UnityEngine;

namespace PocketPlanet.Planet
{
    public enum BiomeKind
    {
        Land,
        Water
    }

    [CreateAssetMenu(menuName = "Pocket Planet/Biome Definition")]
    public class BiomeDefinition : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public BiomeKind Kind;
        public Color NormalColor = Color.white;
        public Color DoomColor = Color.gray;
        public Color HallowedColor = Color.white;
        public Vector2 TemperatureRange = new Vector2(0f, 1f);
        public Vector2 MoistureRange = new Vector2(0f, 1f);
        public bool Buildable = true;
        public string[] Tags = new string[0];

        public bool HasTag(string tag) => Array.IndexOf(Tags, tag) >= 0;

        public bool Matches(float temperature, float moisture) =>
            temperature >= TemperatureRange.x && temperature <= TemperatureRange.y &&
            moisture >= MoistureRange.x && moisture <= MoistureRange.y;
    }
}
