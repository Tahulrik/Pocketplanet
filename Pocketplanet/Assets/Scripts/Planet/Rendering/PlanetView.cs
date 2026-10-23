using UnityEngine;

namespace PocketPlanet.Planet
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PlanetView : MonoBehaviour
    {
        static readonly int BiomeMapId = Shader.PropertyToID("_BiomeMap");
        static readonly int PaletteId = Shader.PropertyToID("_Palette");
        static readonly int BiomeCountId = Shader.PropertyToID("_BiomeCount");
        static readonly int MoodId = Shader.PropertyToID("_Mood");

        [SerializeField] Material material;

        MaterialPropertyBlock properties;
        Texture2D biomeMap;
        Texture2D palette;
        Mesh quad;

        MeshRenderer Renderer => GetComponent<MeshRenderer>();

        public void Render(IPlanet planet)
        {
            Replace(ref biomeMap, BiomePalette.BuildBiomeMap(planet.BlockMap));
            BuildQuad(planet.Frame.Radius, planet.BlockMap.Size * planet.BlockMap.BlockSize);
            Renderer.sharedMaterial = material;
            Properties.SetTexture(BiomeMapId, biomeMap);
            RefreshPalette(planet.Biomes);
        }

        public void RefreshPalette(BiomeTable biomes)
        {
            Replace(ref palette, BiomePalette.BuildTexture(biomes));
            Properties.SetTexture(PaletteId, palette);
            Properties.SetFloat(BiomeCountId, biomes.Count);
            Apply();
        }

        public void SetMood(float mood)
        {
            Properties.SetFloat(MoodId, Mathf.Clamp(mood, -1f, 1f));
            Apply();
        }

        MaterialPropertyBlock Properties => properties ??= new MaterialPropertyBlock();

        void Apply() => Renderer.SetPropertyBlock(Properties);

        // The block grid extends slightly past the circle: uv0 maps the quad onto the grid,
        // uv1 runs -1..1 across the planet diameter for the circle mask.
        void BuildQuad(float radius, float gridExtent)
        {
            if (quad == null)
                quad = new Mesh { name = "PlanetQuad", hideFlags = HideFlags.DontSave };
            var corners = new[] { new Vector2(-1f, -1f), new Vector2(1f, -1f), new Vector2(-1f, 1f), new Vector2(1f, 1f) };
            var vertices = new Vector3[4];
            var gridUv = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                var position = corners[i] * radius;
                vertices[i] = position;
                gridUv[i] = (position + Vector2.one * gridExtent / 2f) / gridExtent;
            }
            quad.Clear();
            quad.vertices = vertices;
            quad.uv = gridUv;
            quad.uv2 = corners;
            quad.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            quad.RecalculateBounds();
            GetComponent<MeshFilter>().sharedMesh = quad;
        }

        static void Replace(ref Texture2D field, Texture2D value)
        {
            if (field != null)
                DestroyImmediate(field);
            value.hideFlags = HideFlags.DontSave;
            field = value;
        }

        void OnDestroy()
        {
            if (biomeMap != null) DestroyImmediate(biomeMap);
            if (palette != null) DestroyImmediate(palette);
            if (quad != null) DestroyImmediate(quad);
        }
    }
}
