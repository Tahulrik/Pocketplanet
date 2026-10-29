using UnityEditor.SceneManagement;
using UnityEngine;

public static class OpenMainScene
{
    public static string Run()
    {
        if (Application.isPlaying)
            return "Refusing: in Play Mode.";
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Main.unity", OpenSceneMode.Single);
        return $"Opened {scene.path}";
    }
}
