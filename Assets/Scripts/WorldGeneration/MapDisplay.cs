using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DrawType
{
    HeightMap = 1,
    Continents = 2,
    MoistureMap = 4,
    TemperatureMap = 8,

    Full = 64
}

public class MapDisplay : MonoBehaviour
{

    public Renderer heightMapRend;

    public void DrawMap(WorldData worldData, DrawType drawMode)
    {
        int width = worldData.worldWidth;
        int height = worldData.worldHeight;

        Texture2D mainTexture = new Texture2D(width, height);
        Texture2D specularMap = new Texture2D(width, height);

        Color[] colourMap = new Color[width * height];
        Color[] specColourMap = new Color[width * height];

        Color water = Color.blue;
        Color land = Color.green;

        Color reflective = new Color(0, 0, 1, 0.4f);
        Color nonReflective = new Color(0, 0, 0, 0f);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colourMap[y * width + x] = Color.Lerp(water, land, worldData.GetNodeDataInWorldAtGridPosition(new Vector2Int(x,y)).NodeHeight);
                specColourMap[y * width + x] = Color.Lerp(reflective, nonReflective, worldData.GetNodeDataInWorldAtGridPosition(new Vector2Int(x, y)).NodeHeight);
            }
        }

        mainTexture.SetPixels(colourMap);
        mainTexture.Apply();
        mainTexture.filterMode = FilterMode.Point;
        mainTexture.wrapMode = TextureWrapMode.Clamp;

        specularMap.SetPixels(specColourMap);
        specularMap.Apply();
        specularMap.filterMode = FilterMode.Point;
        specularMap.wrapMode = TextureWrapMode.Clamp;

        heightMapRend.sharedMaterial.SetTextureScale("_MainTex", new Vector2(0.5f, 0.5f));
        heightMapRend.sharedMaterial.SetTextureOffset("_MainTex", new Vector2(0.5f, 0.5f));

        heightMapRend.sharedMaterial.mainTexture = mainTexture;
        heightMapRend.sharedMaterial.SetTexture("_SpecGlossMap", specularMap);


    }
}
