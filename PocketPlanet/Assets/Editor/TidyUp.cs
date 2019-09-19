using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TidyUp : Editor {

	static string projectPath = "Assets/";

	[MenuItem ("Homemades/Tidy Up All %#t")]
	static void TidyUpAssetsFolder ()
	{
        TidyUpFolder("Scripts", ".cs");
        TidyUpFolder("Scenes", ".unity");
        TidyUpFolder("Prefabs", ".prefab");
        TidyUpFolder("Fonts", ".ttf");
        TidyUpFolder("Graphics", ".png");
        TidyUpFolder("Graphics", ".jpg");
        TidyUpFolder("Sounds", ".mp3");
        TidyUpFolder("Sounds", ".ogg");
        TidyUpFolder("Materials", ".mat");
        TidyUpFolder("Animations/AnimationControllers", ".controller");
        TidyUpFolder("Animations/AnimationClips", ".anim");

		AssetDatabase.Refresh ();
	}

    public static void TidyUpFolder(string folder, string filetype)
    {
        if (!Directory.Exists (projectPath+folder)) {
            Directory.CreateDirectory (projectPath+folder);
        }
        foreach (string file in Directory.GetFiles("Assets")) {
            if (file.Contains (filetype)) 
            {
                string fileName = file.Substring(file.IndexOf('/') + 1);
                File.Move(file, projectPath + folder + "/" + fileName);
            }
        }
    }
}

public class WindowCreate : EditorWindow
{

    private bool[] menuItems = new bool[11];

    [MenuItem ("Homemades/Tidy Up Selected %#&t")]
    static void Init()
    {
        WindowCreate window = ScriptableObject.CreateInstance<WindowCreate>();
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 500, 500);
        window.ShowPopup();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Choose what files you want to TidyUp",EditorStyles.wordWrappedLabel);
        menuItems[0] = EditorGUILayout.Toggle("Scripts", menuItems[0]);
        menuItems[1] = EditorGUILayout.Toggle("Scenes", menuItems[1]);
        menuItems[2] = EditorGUILayout.Toggle("Prefabs", menuItems[2]);
        menuItems[3] = EditorGUILayout.Toggle("Fonts", menuItems[3]);
        menuItems[4] = EditorGUILayout.Toggle("PNG to Graphics", menuItems[4]);
        menuItems[5] = EditorGUILayout.Toggle("JPG to Graphics", menuItems[5]);        
        menuItems[6] = EditorGUILayout.Toggle("MP3 to Sounds", menuItems[6]);
        menuItems[7] = EditorGUILayout.Toggle("OGG to Sounds", menuItems[7]);
        menuItems[8] = EditorGUILayout.Toggle("Materials", menuItems[8]);
        menuItems[9] = EditorGUILayout.Toggle("Animation Controllers", menuItems[9]);
        menuItems[10] = EditorGUILayout.Toggle("Animnation Clips", menuItems[10]);

        GUILayout.Space(70);
        if (GUILayout.Button("Agree!"))
        {
            CheckForTrue(0, "Scripts", ".cs");
            CheckForTrue(1, "Scenes", ".unity");
            CheckForTrue(2, "Prefabs", ".prefab");
            CheckForTrue(3, "Fonts", ".ttf");
            CheckForTrue(4, "Graphics", ".png");
            CheckForTrue(5, "Graphics", ".jpg");            
            CheckForTrue(6, "Sounds", ".mp3");
            CheckForTrue(7, "Sounds", ".ogg");
            CheckForTrue(8, "Materials", ".mat");
            CheckForTrue(9, "Animations/AnimationControllers", ".controller");
            CheckForTrue(10, "Animations/AnimationClips", ".anim");

            AssetDatabase.Refresh();
            this.Close();
        }
            
    }

    void CheckForTrue(int itemNumber, string folderName, string fileEnding)
    {
        if(menuItems[itemNumber] == true)
        {
            TidyUp.TidyUpFolder(folderName, fileEnding);
        }
    }
}
