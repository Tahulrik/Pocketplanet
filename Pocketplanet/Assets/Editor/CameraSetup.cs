using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

// Replaces the old CameraControls camera in Main with the 2019 CameraController rig.
public static class CameraSetup
{
	const string ScenePath = "Assets/Scenes/Main.unity";
	const string RigPrefabPath = "Assets/Scenes/CameraControls/CameraController.prefab";
	const int HiddenLayer = 10;
	static readonly string[] ComponentsNotCopied =
	{
		"Transform", "Camera", "AudioListener", "CameraControls", "UniversalAdditionalCameraData", "CinemachineBrain"
	};

	[MenuItem("Pocket Planet/Use 2019 Camera In Main")]
	public static void SwapMainCamera()
	{
		var scene = EditorSceneManager.OpenScene(ScenePath);
		var objects = AllObjects(scene);
		if (objects.Any(go => go.GetComponent<CameraController>() != null))
		{
			Debug.Log("Main already uses CameraController.");
			return;
		}

		var oldCamera = objects.Select(go => go.GetComponent<Camera>()).FirstOrDefault(c => c != null && c.CompareTag("MainCamera"));
		var rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(RigPrefabPath), scene);
		rig.transform.position = Planet.PlanetObject.transform.position;
		var newCamera = rig.GetComponentsInChildren<Camera>(true).First(c => c.CompareTag("MainCamera"));

		if (oldCamera != null)
			MoveOver(oldCamera, newCamera);
		RemoveOldControls(objects);
		if (oldCamera != null)
			Object.DestroyImmediate(oldCamera.gameObject);

		EditorSceneManager.SaveScene(scene);
		Debug.Log("Main now uses the CameraController rig.");
	}

	static void MoveOver(Camera oldCamera, Camera newCamera)
	{
		newCamera.clearFlags = oldCamera.clearFlags;
		newCamera.backgroundColor = oldCamera.backgroundColor;
		newCamera.cullingMask = ~(1 << HiddenLayer);

		foreach (var component in oldCamera.GetComponents<Component>())
		{
			if (component == null || ComponentsNotCopied.Contains(component.GetType().Name))
				continue;
			ComponentUtility.CopyComponent(component);
			ComponentUtility.PasteComponentAsNew(newCamera.gameObject);
			Debug.Log($"Moved {component.GetType().Name} to the new camera.");
		}

		foreach (var child in oldCamera.transform.Cast<Transform>().ToList())
		{
			child.SetParent(newCamera.transform, false);
			Debug.Log($"Moved child {child.name} to the new camera.");
		}

		KeepSingleAudioListener(newCamera);
	}

	static void KeepSingleAudioListener(Camera newCamera)
	{
		var ownListener = newCamera.GetComponent<AudioListener>();
		var movedListeners = newCamera.GetComponentsInChildren<AudioListener>(true).Where(l => l != ownListener);
		if (ownListener != null && movedListeners.Any())
			Object.DestroyImmediate(ownListener);
	}

	static void RemoveOldControls(List<GameObject> objects)
	{
		foreach (var controls in objects.Select(go => go.GetComponent<CameraControls>()).Where(c => c != null).ToList())
		{
			var owner = controls.gameObject;
			Object.DestroyImmediate(controls);
			if (owner.GetComponents<Component>().Length == 1 && owner.transform.childCount == 0)
				Object.DestroyImmediate(owner);
		}
	}

	static List<GameObject> AllObjects(Scene scene) =>
		scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject).ToList();
}
