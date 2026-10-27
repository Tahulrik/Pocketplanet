using PocketPlanet.Planet.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildRowControlsSetup
{
	const string PrefabPath = "Assets/Prefabs/BuildRowControls.prefab";
	const string ButtonSpritePath = "Assets/Graphics/Other/Button_Standart_01.png";

	[MenuItem("Pocket Planet/Add Build Row Controls To Game Scenes")]
	public static void AddToGameScenes()
	{
		var prefab = CreatePrefab();
		foreach (var scenePath in PlanetSetup.GameScenes)
			AddToScene(scenePath, prefab);
	}

	// Batch entry point: Unity -batchmode -executeMethod BuildRowControlsSetup.SetUpEverything -quit
	public static void SetUpEverything()
	{
		AssetDatabase.Refresh();
		PlanetDecorSetup.AddToPlanetPrefab();
		AddToGameScenes();
		AssetDatabase.SaveAssets();
	}

	static GameObject CreatePrefab()
	{
		var root = new GameObject("BuildRowControls");
		try
		{
			root.AddComponent<BuildRowSelection>();
			root.AddComponent<BuildRowKeyboardInput>();
			root.AddComponent<EnsureEventSystem>();
			var buttons = new SerializedObject(root.AddComponent<BuildRowButtons>());
			buttons.FindProperty("buttonBackground").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpritePath);
			buttons.ApplyModifiedPropertiesWithoutUndo();
			return PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
		}
		finally
		{
			Object.DestroyImmediate(root);
		}
	}

	static void AddToScene(string scenePath, GameObject prefab)
	{
		var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
		foreach (var root in scene.GetRootGameObjects())
			if (PrefabUtility.GetCorrespondingObjectFromSource(root) == prefab)
				return;
		PrefabUtility.InstantiatePrefab(prefab, scene);
		EditorSceneManager.SaveScene(scene);
		Debug.Log($"Added build row controls to {scenePath}.");
	}
}
