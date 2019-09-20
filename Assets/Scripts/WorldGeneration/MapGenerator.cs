using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int CircleSegmentCount = 64;

    MeshFilter filter;
    Renderer rend;

    [Range(0,1000)]
    public int MapWidth = 100;
    [Range(0,1000)]
    public int MapHeight = 100;
    [Range(0f, 1f)]
    public float MapResolution = 0.6f;


    [Range(1f, 100f)]
    public float radius;

    public void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            rend = gameObject.AddComponent<MeshRenderer>();
        }

        filter = GetComponent<MeshFilter>();
        if (filter == null)
        {
            filter = gameObject.AddComponent<MeshFilter>();
        }
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            var mapData = NoiseGenerator.GenerateNoiseMap(MapWidth, MapHeight, MapResolution);

            var tex = GenerateMapTexture(mapData);
            filter.mesh = GenerateCircleMesh(CircleSegmentCount, radius);
            rend.sharedMaterial.mainTexture = tex;

        }
    }




    private Texture2D GenerateMapTexture(float[,] dataArray)
    {
        int width = dataArray.GetLength(0);
        int height = dataArray.GetLength(1);

        Texture2D resultTexture = new Texture2D(width, height);
        
        Color[] colourMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                colourMap[y * width + x] = dataArray[x,y] < 0.5f ? Color.white : Color.black;
            }
        }

        resultTexture.SetPixels(colourMap);
        resultTexture.Apply();
        
        return resultTexture;
    }

    private Mesh GenerateCircleMesh(int segments, float radius)
    {
        int CircleVertexCount = segments + 2;
        int CircleIndexCount = segments * 3;

        var circle = new Mesh();
        var vertices = new List<Vector3>(CircleVertexCount);
        var indices = new int[CircleIndexCount];
        Vector2[] uvs = new Vector2[CircleVertexCount];
        print(uvs.Length);
        var segmentWidth = Mathf.PI * 2f / segments;
        var angle = 0f;
        vertices.Add(Vector3.zero);
        uvs[0] = Vector2.zero;
        for (int i = 1; i < CircleVertexCount; ++i)
        {
            vertices.Add(new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius);
            uvs[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            angle -= segmentWidth;
            if (i > 1)
            {
                var j = (i - 2) * 3;
                indices[j + 0] = 0;
                indices[j + 1] = i - 1;
                indices[j + 2] = i;
            }
        }

        circle.SetVertices(vertices);
        circle.SetIndices(indices, MeshTopology.Triangles, 0);
        circle.SetUVs(0, uvs);
        circle.RecalculateNormals();
        circle.RecalculateBounds();

        circle.name = "CircleMesh";
        return circle;
    }
}
