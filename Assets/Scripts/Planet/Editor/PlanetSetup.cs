using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace PocketPlanet.Planet.Editor
{
    public static class PlanetSetup
    {
        public const string Root = "Assets/Planet";
        public const string PrefabPath = Root + "/ProceduralPlanet.prefab";
        const string BiomesFolder = Root + "/Biomes";
        const string SettingsPath = Root + "/DefaultPlanetSettings.asset";
        const string MaterialPath = Root + "/PlanetSurface.mat";
        const string ShaderName = "PocketPlanet/PlanetSurface";
        const int PlanetSortingOrder = -10;
        const string OldPlanetPrefabPath = "Assets/Prefabs/Planet.prefab";
        const string LegacyPlanetObjectName = "Planet";
        static readonly string[] OldGeneratorTypes = { "MapDisplay", "MapGenerator" };
        static readonly string[] GameScenes =
        {
            "Assets/Scenes/Main.unity",
            "Assets/Scenes/Development_Main.unity",
            "Assets/Scenes/CameraControls/Development_CameraControls.unity"
        };

        [MenuItem("Pocket Planet/Create Default Planet Assets")]
        public static void CreateDefaultAssets()
        {
            EnsureFolder(Root);
            EnsureFolder(BiomesFolder);
            var biomes = DefaultBiomes.Create().Select(b => LoadOrCreate(b, $"{BiomesFolder}/{b.Id}.asset")).ToList();
            var settings = LoadOrCreateSettings(biomes);
            var material = LoadOrCreateMaterial();
            CreatePrefab(settings, material);
            AssetDatabase.SaveAssets();
            Debug.Log($"Planet assets ready in {Root}.");
        }

        [MenuItem("Pocket Planet/Replace Old Planets In Game Scenes")]
        public static void ReplacePlanetsInGameScenes()
        {
            foreach (var scenePath in GameScenes)
                ReplacePlanetInScene(scenePath);
        }

        public static void ReplacePlanetInScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var oldObjects = OldPlanetObjects(scene).ToList();
            if (oldObjects.Count == 0)
            {
                Debug.LogWarning($"No old planet found in {scenePath}.");
                return;
            }

            var position = oldObjects[0].transform.position;
            foreach (var oldObject in oldObjects)
                Object.DestroyImmediate(oldObject);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var planet = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            planet.name = LegacyPlanetObjectName;
            planet.transform.position = position;
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Replaced {oldObjects.Count} old planet object(s) in {scenePath}.");
        }

        static IEnumerable<GameObject> OldPlanetObjects(Scene scene)
        {
            var all = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject).ToList();
            var prefabInstances = all.Where(IsOldPlanetPrefabInstance);
            var generators = OldGeneratorTypes.SelectMany(typeName => all.Where(go => HasComponentNamed(go, typeName)));
            return prefabInstances.Concat(generators).Distinct();
        }

        static bool IsOldPlanetPrefabInstance(GameObject go) =>
            PrefabUtility.IsOutermostPrefabInstanceRoot(go)
            && PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go) == OldPlanetPrefabPath;

        static bool HasComponentNamed(GameObject go, string typeName) =>
            go.GetComponents<MonoBehaviour>().Any(c => c != null && c.GetType().Name == typeName);

        static void CreatePrefab(PlanetSettings settings, Material material)
        {
            var root = new GameObject("ProceduralPlanet") { tag = "Water" };
            try
            {
                var view = root.AddComponent<PlanetView>();
                SetField(view, "material", material);
                root.GetComponent<MeshRenderer>().sortingOrder = PlanetSortingOrder;
                root.AddComponent<PlanetHost>().Configure(settings, view);
                AddMood(root, view);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        static void AddMood(GameObject root, PlanetView view)
        {
            var volumeObject = new GameObject("MoodVolume");
            volumeObject.transform.SetParent(root.transform, false);
            var volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;

            var source = root.AddComponent<ManualMoodSource>();
            var palette = root.AddComponent<PlanetPaletteMoodEffect>();
            SetField(palette, "view", view);
            var grading = root.AddComponent<GlobalColorMoodEffect>();
            SetField(grading, "volume", volume);

            var controller = root.AddComponent<WorldMoodController>();
            SetField(controller, "source", source);
            var serialized = new SerializedObject(controller);
            var effects = serialized.FindProperty("effects");
            effects.arraySize = 2;
            effects.GetArrayElementAtIndex(0).objectReferenceValue = palette;
            effects.GetArrayElementAtIndex(1).objectReferenceValue = grading;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static T LoadOrCreate<T>(T instance, string path) where T : Object
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
                return existing;
            AssetDatabase.CreateAsset(instance, path);
            return instance;
        }

        static PlanetSettings LoadOrCreateSettings(List<BiomeDefinition> biomes)
        {
            var settings = ScriptableObject.CreateInstance<PlanetSettings>();
            settings.BiomeTable = biomes;
            return LoadOrCreate(settings, SettingsPath);
        }

        static Material LoadOrCreateMaterial()
        {
            var shader = Shader.Find(ShaderName);
            if (shader == null)
                throw new System.InvalidOperationException($"Shader '{ShaderName}' not found.");
            return LoadOrCreate(new Material(shader), MaterialPath);
        }

        static void SetField(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            var parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }
    }
}
