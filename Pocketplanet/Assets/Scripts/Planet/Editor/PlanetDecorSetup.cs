using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PocketPlanet.Planet.Editor
{
    public static class PlanetDecorSetup
    {
        const string TreesName = "Trees";
        const string SkyName = "Sky";
        const string MountainsName = "Mountains";
        static readonly string[] MountainSprites = { "Mountain_01", "Mountain_02", "Mountain_03" };
        const string NatureFolder = "Assets/Graphics/Nature";
        const string CloudFolder = "Assets/Graphics/Planet";
        const int CloudSpriteCount = 7;

        static readonly (string Biome, float Density, string[] Trees)[] FloraByBiome =
        {
            ("Grassland", 0.8f, new[] { "Tree_Round_01", "Tree_Round_02", "Tree_Bush_01" }),
            ("Forest", 2.2f, new[] { "Tree_Round_01", "Tree_Pine_01", "Tree_Poplar_01", "Tree_Round_02" }),
            ("Desert", 0.5f, new[] { "Tree_Cactus_01", "Tree_Bush_Dry_01" }),
            ("Snow", 1f, new[] { "Tree_Pine_Snow_01", "Tree_Pine_01" })
        };

        [MenuItem("Pocket Planet/Add Trees And Clouds To Planet Prefab")]
        public static void AddToPlanetPrefab()
        {
            var root = PrefabUtility.LoadPrefabContents(PlanetSetup.PrefabPath);
            try
            {
                Add(root);
                PrefabUtility.SaveAsPrefabAsset(root, PlanetSetup.PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            Debug.Log("Trees and clouds added to the planet prefab.");
        }

        public static void Add(GameObject planetRoot)
        {
            AddGround(planetRoot);
            AddSky(ReplaceChild(planetRoot, SkyName), planetRoot.GetComponent<WorldMoodController>());
        }

        [MenuItem("Pocket Planet/Refresh Trees And Mountains On Planet Prefab")]
        public static void RefreshGroundOnPlanetPrefab()
        {
            var root = PrefabUtility.LoadPrefabContents(PlanetSetup.PrefabPath);
            try
            {
                AddGround(root);
                PrefabUtility.SaveAsPrefabAsset(root, PlanetSetup.PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            Debug.Log("Trees and mountains refreshed on the planet prefab.");
        }

        static void AddGround(GameObject planetRoot)
        {
            AddTrees(ReplaceChild(planetRoot, TreesName));
            AddMountains(ReplaceChild(planetRoot, MountainsName));
        }

        static void AddMountains(GameObject holder)
        {
            var view = new SerializedObject(holder.AddComponent<MountainView>());
            SetSprites(view.FindProperty("sprites"), MountainSprites.Select(m => $"{NatureFolder}/{m}.png"));
            view.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddTrees(GameObject holder)
        {
            var grove = new SerializedObject(holder.AddComponent<TreeGrove>());
            var flora = grove.FindProperty("flora");
            flora.arraySize = FloraByBiome.Length;
            for (int i = 0; i < FloraByBiome.Length; i++)
            {
                var entry = flora.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("biomeId").stringValue = FloraByBiome[i].Biome;
                entry.FindPropertyRelative("density").floatValue = FloraByBiome[i].Density;
                SetSprites(entry.FindPropertyRelative("sprites"), FloraByBiome[i].Trees.Select(t => $"{NatureFolder}/{t}.png"));
            }
            grove.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddSky(GameObject holder, WorldMoodController mood)
        {
            var sky = new SerializedObject(holder.AddComponent<CloudSky>());
            var cloudPaths = Enumerable.Range(1, CloudSpriteCount).Select(i => $"{CloudFolder}/Cloud_{i:000}.png");
            SetSprites(sky.FindProperty("cloudSprites"), cloudPaths);
            sky.FindProperty("mood").objectReferenceValue = mood;
            sky.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetSprites(SerializedProperty array, System.Collections.Generic.IEnumerable<string> paths)
        {
            var sprites = paths.Select(LoadSprite).ToArray();
            array.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        static Sprite LoadSprite(string path)
        {
            var sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
            if (sprite == null)
                Debug.LogWarning($"No sprite found at {path}. Is it imported as a Sprite?");
            return sprite;
        }

        static GameObject ReplaceChild(GameObject parent, string name)
        {
            var existing = parent.transform.Find(name);
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            return child;
        }
    }
}
