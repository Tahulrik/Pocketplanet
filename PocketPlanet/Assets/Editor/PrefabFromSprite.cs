using System.IO;
using UnityEngine;
using UnityEditor;

public class PrefabFromSprite : Editor 
{
	[MenuItem ("Homemades/Prefab All Sprites")]
	static void PrefabAllSprites()
	{
		foreach(Sprite s in Resources.LoadAll<Sprite>("CitizenParts"))
		{
			string spritePath = AssetDatabase.GetAssetPath(s);
			string prefabPath = spritePath.Replace(".png", ".prefab");
			string directoryPath = Path.GetDirectoryName(spritePath);
			string directoryName = directoryPath.Substring(directoryPath.LastIndexOf('/') + 1);
			AssetDatabase.DeleteAsset(prefabPath);
			GameObject go = new GameObject();
			AddCorrectComponent(directoryName, go);
			go.AddComponent<SpriteRenderer>().sprite = s;
			PrefabUtility.CreatePrefab(prefabPath, go);
			DestroyImmediate(go);
		}
	}
	[MenuItem ("Homemades/Prefab Selected Sprites")]
	static void PrefabSelectedSprites()
	{
		foreach(Object s in Selection.objects)
		{
			GameObject go = null;
			try
			{
				string spritePath = AssetDatabase.GetAssetPath(s);
				Object[] tempSprite = AssetDatabase.LoadAllAssetRepresentationsAtPath(spritePath);
				string prefabPath = spritePath.Replace(".png", ".prefab");
				string directoryPath = Path.GetDirectoryName(spritePath);
				string directoryName = directoryPath.Substring(directoryPath.LastIndexOf('/') + 1);
				go = new GameObject();
				AddCorrectComponent(directoryName, go);
				go.AddComponent<SpriteRenderer>().sprite = tempSprite[0] as Sprite;
				AssetDatabase.DeleteAsset(prefabPath);
				PrefabUtility.CreatePrefab(prefabPath, go);
				DestroyImmediate(go);
			}
			catch(System.Exception)
			{
				Debug.LogError("Selection is not a sprite");

				DestroyImmediate(go);
				break;
			}
		}
	}
	public static void AddCorrectComponent(string scriptName, GameObject go)
	{
		if(scriptName == "BodyAccessory")
			go.AddComponent<BodyAccessory>();
		else if(scriptName == "FaceAccessory")
			go.AddComponent<FaceAccessory>();
		else if(scriptName == "GlassesAccessory")
			go.AddComponent<GlassesAccessory>();
		else if(scriptName == "HatAccessory")
			go.AddComponent<HatAccessory>();
		else if(scriptName == "BodyType")
			go.AddComponent<BodyType>();
		else if(scriptName == "Eyes")
			go.AddComponent<Eyes>();
		else if(scriptName == "Hair")
			go.AddComponent<Hair>();
		else if(scriptName == "Jacket")
			go.AddComponent<Jacket>();
		else if(scriptName == "Pants")
			go.AddComponent<Pants>();
		else if(scriptName == "Shirt")
			go.AddComponent<Shirt>();
		else if(scriptName == "Shoe")
			go.AddComponent<Shoe>();
		else if(scriptName == "Suit")
			go.AddComponent<Suit>();
		else if(scriptName == "Special")
			go.AddComponent<Special>();
	}
}
