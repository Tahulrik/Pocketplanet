using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MapGenerator : MonoBehaviour
{
    public int CircleSegmentCount = 64;

    MeshFilter filter;
    Renderer rend;

    [Range(1f, 100f)]
    public int LandmassBlockSize = 1;

    [Range(5f, 50f)]
    public float NoiseScale = 25f;

    [Range(1, 12)]
    public int Octaves = 4;
    [Range(0.1f, 10f)]
    public float Persistance = 0.5f;
    [Range(0.1f, 10f)]
    public float Lacunarity = 2.0f;

    [Range(1f, 100f)]
    public float Radius = 3;
    [Range(0, 1000)]
    public int PixelDensity = 100;

    public int Seed;
    public Vector2 Offset;
    public bool ThresholdMap = false;
    public bool AutoUpdate = false;
    public bool SquarizeWorld = false;

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



        //filter.mesh = ;

    }
    public void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Mouse0))
        {


        }
    }

    public void Generate()
    {
        var mapData = NoiseGenerator.GenerateNoiseMap((int)Radius * PixelDensity, (int)Radius * PixelDensity, Seed, NoiseScale, Octaves, Persistance, Lacunarity, Offset);
        if(SquarizeWorld)
            mapData = SquarizeWorldData(mapData, LandmassBlockSize);

        MapDisplay display = FindObjectOfType<MapDisplay>();

        
        var mesh = display.gameObject.GetComponentInChildren<MeshFilter>();
        if(mesh.sharedMesh == null)
            mesh.sharedMesh = GenerateCircleMesh(CircleSegmentCount, Radius);

        display.DrawNoiseMap(mapData, ThresholdMap);
    }

    private float[,] SquarizeWorldData(float[,] mapData, int kernelSize)
    {
        float[,] squarizedMap = (float[,])mapData.Clone();
        int mapX = mapData.GetLength(0);
        int mapY = mapData.GetLength(1);


        for (int y = 0; y < mapY; y += (kernelSize*2)+1)
        {
            for (int x = 0; x < mapX; x += (kernelSize*2)+1)
            {
                squarizedMap = ApplySquareKernel(squarizedMap, x, y, kernelSize);
            }
        }

        return squarizedMap;
    }

    private float[,] ApplySquareKernel(float[,] inputMap, int x, int y, int kernelSize)
    {
        float[,] outputMap = (float[,])inputMap.Clone();
        int mapX = outputMap.GetLength(0);
        int mapY = outputMap.GetLength(1);

        int kernelCenterX = x;
        int kernelCenterY = y;
        float resultval = inputMap[kernelCenterX, kernelCenterY] < 0.5f ? 0f : 1f;
        for (int kernelY = -kernelSize; kernelY <= kernelSize * 2; kernelY++)
        {
            int kernelPosY = kernelCenterY + kernelY;
            if (kernelPosY < 0 || kernelPosY >= mapY)
                continue;

            for (int kernelX = -kernelSize; kernelX <= kernelSize * 2; kernelX++)
            {
                int kernelPosX = kernelCenterX + kernelX;

                if (kernelPosX < 0 || kernelPosX >= mapX)
                    continue;

                outputMap[kernelPosX, kernelPosY] = resultval;
            }
        }

        return outputMap;
    }

    private Mesh GenerateCircleMesh(int segments, float radius)
    {
        int CircleVertexCount = segments + 2;
        int CircleIndexCount = segments * 3;

        var circle = new Mesh();
        var vertices = new List<Vector3>(CircleVertexCount);
        var indices = new int[CircleIndexCount];
        Vector2[] uvs = new Vector2[CircleVertexCount];
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
