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

    public Renderer PlanetRenderer;
    public Renderer SkyRenderer;

    public void DrawMap(float[,] heightMap, DrawType drawMode)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

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
                colourMap[y * width + x] = Color.Lerp(water, land, heightMap[x,y]);
                specColourMap[y * width + x] = Color.Lerp(reflective, nonReflective, heightMap[x, y]);
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

        PlanetRenderer.sharedMaterial.SetTextureScale("_BaseMap", new Vector2(0.5f, 0.5f));
        PlanetRenderer.sharedMaterial.SetTextureOffset("_BaseMap", new Vector2(0.5f, 0.5f));

        PlanetRenderer.sharedMaterial.SetTexture("_BaseMap", mainTexture);
        PlanetRenderer.sharedMaterial.SetTexture("_SpecGlossMap", specularMap);


    }

    public void DrawSky(WorldNode[,] mapData)
    {
        var height = mapData.GetLength(0);

        Texture2D mainTexture = new Texture2D(height, height);

        Color[] colourMap = new Color[height * height];

        Color resColor = Color.clear;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < height; x++)
            {
                WorldNode currentNode = mapData[x, y];
                if (currentNode.nodeType == WorldNodeType.Air)
                {
                    resColor = new Color(0.4f* currentNode.NodeHeight, 0.4f *currentNode.NodeHeight, 1* currentNode.NodeHeight, currentNode.NodeHeight);
                }
                else if (currentNode.nodeType == WorldNodeType.Land || currentNode.nodeType == WorldNodeType.Water)
                {
                    resColor = new Color(0.4f, 0.4f, 1, 1);
                }
                else
                {
                    continue;
                }

                colourMap[y * height + x] = resColor;
            }
        }

        mainTexture.SetPixels(colourMap);
        mainTexture.Apply();
        mainTexture.filterMode = FilterMode.Bilinear;
        mainTexture.wrapMode = TextureWrapMode.Clamp;

        SkyRenderer.sharedMaterial.SetTexture("_BaseMap", mainTexture);
    }
}
