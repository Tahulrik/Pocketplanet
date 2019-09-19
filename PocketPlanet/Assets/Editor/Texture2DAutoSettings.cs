using UnityEngine;
using UnityEditor;

class Texture2DAutoSettings : AssetPostprocessor {
    void OnPreprocessTexture () {
		if (assetPath.Contains("CitizenParts") || assetPath.Contains("Graphics") && !assetPath.Contains("Planet") && !assetPath.Contains("NonChangeable")) {
            Debug.Log ("Importing new sprite!");
            TextureImporter myTextureImporter  = (TextureImporter)assetImporter;
            myTextureImporter.textureType = TextureImporterType.Sprite;
            myTextureImporter.textureShape = TextureImporterShape.Texture2D;
            myTextureImporter.spriteImportMode = SpriteImportMode.Single;
            myTextureImporter.spritePixelsPerUnit = 1000;
            myTextureImporter.ClearPlatformTextureSettings("Web");
            myTextureImporter.ClearPlatformTextureSettings("Standalone");
            myTextureImporter.ClearPlatformTextureSettings("iPhone");
        }
    }
}