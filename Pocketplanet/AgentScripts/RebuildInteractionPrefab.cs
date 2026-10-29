using UnityEditor;
using UnityEngine;

public static class RebuildInteractionPrefab
{
    public static string Run()
    {
        if (Application.isPlaying)
            return "Refusing: editor is in Play Mode.";
        var prefab = GameControlsSetup.CreateInteractionPrefab();
        AssetDatabase.SaveAssets();
        var components = string.Join(", ", System.Array.ConvertAll(prefab.GetComponents<Component>(), c => c.GetType().Name));
        return $"Rebuilt {AssetDatabase.GetAssetPath(prefab)}: {components}";
    }
}
