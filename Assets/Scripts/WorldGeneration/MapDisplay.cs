using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{

    public Renderer heightMapRend;

    public void DrawNoiseMap(float[,] dataArray, bool thresholdValues)
    {
        int width = dataArray.GetLength(0);
        int height = dataArray.GetLength(1);

        Texture2D mainTexture = new Texture2D(width, height);
        Texture2D specularMap = new Texture2D(width, height);

        Color[] colourMap = new Color[width * height];
        Color[] specColourMap = new Color[width * height];

        Color water = Color.blue;
        Color land = Color.green;

        Color reflective = new Color(0, 0, 1, 1);
        Color nonReflective = new Color(0, 1, 0, 0);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (thresholdValues)
                {
                    colourMap[y * width + x] = (dataArray[x, y] < 0.5f) ? land : water;
                    specColourMap[y * width + x] = (dataArray[x, y] < 0.5f) ? nonReflective : reflective;
                }
                else
                {
                    colourMap[y * width + x] = Color.Lerp(land, water, dataArray[x, y]);
                    specColourMap[y * width + x] = Color.Lerp(nonReflective, reflective, dataArray[x, y]);
                }

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

        heightMapRend.sharedMaterial.mainTexture = mainTexture;
        heightMapRend.sharedMaterial.SetTexture("_SpecGlossMap",specularMap);
    }
}
