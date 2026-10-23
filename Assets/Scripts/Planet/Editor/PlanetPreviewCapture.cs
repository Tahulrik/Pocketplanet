using System.IO;
using UnityEditor;
using UnityEngine;

namespace PocketPlanet.Planet.Editor
{
    // Renders the planet prefab to PNG files so generation and mood can be checked without opening a scene.
    public static class PlanetPreviewCapture
    {
        const int Resolution = 512;
        static readonly float[] Moods = { -1f, 0f, 1f };

        [MenuItem("Pocket Planet/Capture Planet Previews")]
        public static void Capture()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlanetSetup.PrefabPath);
            var planetObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var cameraObject = new GameObject("PreviewCamera");
            var texture = new RenderTexture(Resolution, Resolution, 24);
            try
            {
                var planet = planetObject.GetComponent<PlanetHost>().Regenerate();
                var camera = SetUpCamera(cameraObject, planet.Frame.Radius, texture);
                var view = planetObject.GetComponent<PlanetView>();
                foreach (var mood in Moods)
                {
                    view.SetMood(mood);
                    camera.Render();
                    Save(texture, $"Logs/planet-preview-mood{mood:+0;-0;0}.png");
                }
            }
            finally
            {
                Object.DestroyImmediate(planetObject);
                Object.DestroyImmediate(cameraObject);
                texture.Release();
            }
        }

        static Camera SetUpCamera(GameObject cameraObject, float radius, RenderTexture target)
        {
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = radius * 1.15f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.06f, 0.12f);
            camera.targetTexture = target;
            return camera;
        }

        static void Save(RenderTexture source, string path)
        {
            var previous = RenderTexture.active;
            RenderTexture.active = source;
            var image = new Texture2D(source.width, source.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            image.Apply();
            RenderTexture.active = previous;
            File.WriteAllBytes(path, image.EncodeToPNG());
            Object.DestroyImmediate(image);
            Debug.Log($"Saved planet preview to {path}");
        }
    }
}
