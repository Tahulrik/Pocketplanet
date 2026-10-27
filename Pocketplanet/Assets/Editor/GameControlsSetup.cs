using System;
using PocketPlanet.Planet;
using PocketPlanet.Planet.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class GameControlsSetup
{
	const string BuildRowPrefabPath = "Assets/Prefabs/BuildRowControls.prefab";
	const string InteractionPrefabPath = "Assets/Prefabs/WorldInteraction.prefab";

	[MenuItem("Pocket Planet/Add Game Controls To Game Scenes")]
	public static void AddToGameScenes()
	{
		var buildRows = CreatePrefab(BuildRowPrefabPath, "BuildRowControls", root =>
		{
			root.AddComponent<BuildRowSelection>();
			root.AddComponent<BuildRowDesktopInput>();
			root.AddComponent<BuildRowSwipeInput>();
			root.AddComponent<BuildRowToast>();
			root.AddComponent<EnsureEventSystem>();
		});
		var interaction = CreatePrefab(InteractionPrefabPath, "WorldInteraction", root =>
		{
			root.AddComponent<RadialContextMenu>();
			root.AddComponent<WorldTapSelector>();
		});

		foreach (var scenePath in PlanetSetup.GameScenes)
			AddToScene(scenePath, buildRows, interaction);
	}

	// Batch entry point: Unity -batchmode -executeMethod GameControlsSetup.SetUpEverything -quit
	public static void SetUpEverything()
	{
		AssetDatabase.Refresh();
		PlanetDecorSetup.AddToPlanetPrefab();
		AddToGameScenes();
		AssetDatabase.SaveAssets();
	}

	static GameObject CreatePrefab(string path, string name, Action<GameObject> build)
	{
		var root = new GameObject(name);
		try
		{
			build(root);
			return PrefabUtility.SaveAsPrefabAsset(root, path);
		}
		finally
		{
			Object.DestroyImmediate(root);
		}
	}

	static void AddToScene(string scenePath, params GameObject[] prefabs)
	{
		var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
		foreach (var prefab in prefabs)
			if (!HasInstance(scene, prefab))
				PrefabUtility.InstantiatePrefab(prefab, scene);
		EditorSceneManager.SaveScene(scene);
		Debug.Log($"Game controls are in {scenePath}.");
	}

	static bool HasInstance(UnityEngine.SceneManagement.Scene scene, GameObject prefab)
	{
		foreach (var root in scene.GetRootGameObjects())
			if (PrefabUtility.GetCorrespondingObjectFromSource(root) == prefab)
				return true;
		return false;
	}
}
