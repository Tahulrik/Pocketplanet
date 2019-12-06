﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MapGenerator : MonoBehaviour
{
    #region properties
    public int CircleSegmentCount = 64;

    MeshFilter filter;
    Renderer rend;

    public DrawType MapDrawMode = DrawType.Full;

    [Range(1f, 100f)]
    public int LandmassBlockSize = 1;

    [Range(50f, 100f)]
    public float NoiseScale = 50f;

    [Range(1, 12)]
    public int Octaves = 4;
    [Range(0.1f, 10f)]
    public float Persistance = 0.5f;
    [Range(0.1f, 10f)]
    public float Lacunarity = 2.0f;

    [Range(1f, 100f)]
    public float Radius = 3;
    [Range(1, 300)]
    public int PixelPerDistance = 100;
    int _totalPixelsOnDiameter;
    public float _pixelSize;

    public int Seed;
    public Vector2 Offset;

    [Range(20,200)]
    public int LandDepth;
    int _calculatedLandDepth;
    [Range(1,8)]
    public int Continents = 3;
    [Range(40, 95)]
    public int LandCoveragePercentage = 80;
    public int _worldCircumference;
    public List<int> _continentsCoverageData;

    [Range(0, 360)]
    public int ContinentsOffset = 200;

    int totalPossibleLandCoverage;
    int totalPossibleWaterCoverage;
    int minimumViableLandSize;
    int minimumViableOceanSize;

    int randomContinentVariationFraction = 0;
    int continentSizeRandomVal = 0;
    #endregion

    #region MonobehaviourMethods
    public void Awake()
    {
        Generate();
    }
    public void Start()
    { 

    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            var dist = Vector3.Distance(Camera.main.transform.position, rend.transform.position);
            var screenPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, dist));
            var planetpos = rend.transform.position;
            Vector2 posInMapCoords = new Vector2(screenPos.x - planetpos.x, screenPos.y - planetpos.y);


            Debug.DrawLine(planetpos, posInMapCoords, Color.red, 5);
            Debug.DrawLine(Camera.main.transform.position, screenPos, Color.blue, 5);

            var node = WorldData.instance.GetNodeDataAtWorldPosition(posInMapCoords);
            print("inworldPos" + posInMapCoords + " " + node.nodeType.ToString());

        }
    }
    void OnValidate()
    {
        _worldCircumference = (int)((2 * (Radius * PixelPerDistance)) * Mathf.PI);
        _continentsCoverageData = new List<int>(Continents);
        _totalPixelsOnDiameter = (int)(Radius * 2) * PixelPerDistance;
        _pixelSize = (Radius) / (PixelPerDistance * _totalPixelsOnDiameter);
        _calculatedLandDepth = LandDepth * (int)Radius;
    }
    #endregion

    public void UpdateRandomVarianceData()
    {
        Continents = Random.Range(2, 5);
        _continentsCoverageData = new List<int>(Continents);

        totalPossibleLandCoverage = (int)((_worldCircumference / 100f) * LandCoveragePercentage);
        totalPossibleWaterCoverage = _worldCircumference - totalPossibleLandCoverage;

        minimumViableLandSize = (LandmassBlockSize * 2) + Mathf.RoundToInt(totalPossibleLandCoverage / 100f) * 5;
        minimumViableOceanSize = (LandmassBlockSize * 2) + Mathf.RoundToInt(totalPossibleWaterCoverage / 100f) * (100 / (Continents + 1));

        randomContinentVariationFraction = (int)(totalPossibleLandCoverage / 100f) * 5;
        ContinentsOffset = Random.Range(0, 360);
    }

    public void GenerateWorldSurfaceData()
    {
        int remainingLandCoverage = totalPossibleLandCoverage;
        int remainingWaterCoverage = totalPossibleWaterCoverage;


        for (int i = 0; i < Continents; i++)
        {
            continentSizeRandomVal = Random.Range(-randomContinentVariationFraction, randomContinentVariationFraction);

            if (i == Continents - 1)
            {
                if (remainingLandCoverage < minimumViableLandSize)
                {
                    int missingAmount = minimumViableLandSize - remainingLandCoverage;

                    var selectedContinent = _continentsCoverageData.First(x => x > missingAmount && i == 0 || i % 2 == 0);
                    selectedContinent -= missingAmount;
                    remainingLandCoverage += missingAmount;
                }

                if (remainingWaterCoverage < minimumViableOceanSize)
                {
                    int missingAmount = minimumViableOceanSize - remainingWaterCoverage;

                    var selectedContinent = _continentsCoverageData.First(x => x > missingAmount && i % 2 == 1);
                    selectedContinent -= missingAmount;
                    remainingWaterCoverage += missingAmount;
                }

                _continentsCoverageData.Add(remainingLandCoverage);
                _continentsCoverageData.Add(remainingWaterCoverage);
                break;
            }

            int newContinentSize = 0;
            int newOceanSize = 0;

            newContinentSize = Mathf.RoundToInt(totalPossibleLandCoverage / Continents) + continentSizeRandomVal;

            remainingLandCoverage -= newContinentSize;
            _continentsCoverageData.Add(remainingLandCoverage);

            newOceanSize = remainingWaterCoverage / 4 + minimumViableLandSize;
            remainingWaterCoverage -= newOceanSize;
            _continentsCoverageData.Add(remainingWaterCoverage);
        }

    }

    void GenerateWorldDataFromHeightMap(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var value = heightMap[x, y];
                WorldData.instance.SetNodeDataInWorldAtGridPosition(
                    new WorldNode(value, (value < 1) ? WorldNodeType.Water : WorldNodeType.Land),
                    new Vector2Int(x, y));
            }
        }
    }

    float[,] DrawContinentsOnMap(float[,] mapData)
    {
        float angle = 0f;

        int currentContinent = _continentsCoverageData.First();
        int continentProgress = 0;
        int continentPixelProgress = 0;
        bool DrawingContinent = true;
        bool HasSelectedDrawType = true;
        float pixelDrawVal = 0;

        for (int i = 0 + ContinentsOffset; i <= _worldCircumference + ContinentsOffset; ++i)
        {
            if (currentContinent <= continentPixelProgress)
            {
                HasSelectedDrawType = false;
            }

            if (!HasSelectedDrawType)
            {
                if (DrawingContinent)
                {
                    DrawingContinent = false;
                }
                else
                {
                    DrawingContinent = true;
                }

                continentProgress++;
                if (continentProgress < _continentsCoverageData.Count)
                    currentContinent = _continentsCoverageData[continentProgress];
                continentPixelProgress = 0;
                HasSelectedDrawType = true;
            }

            for (int depth = -_calculatedLandDepth; depth <= _calculatedLandDepth; depth++)
            {
                var currentDist = Radius - (depth * _pixelSize);
                var pixelLocationOnWorldEdge = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle))
                    * (PixelPerDistance * currentDist);

                pixelDrawVal = (DrawingContinent) ? 1 : 0;

                var maxX = mapData.GetLength(0) - 1;
                var maxY = mapData.GetLength(1) - 1;

                var xInMap = Mathf.Clamp(Mathf.RoundToInt((((maxX + 1) / 2f) + (int)pixelLocationOnWorldEdge.x) - 0.5f), 0, maxX);
                var yInMap = Mathf.Clamp(Mathf.RoundToInt((((maxY + 1) / 2f) + (int)pixelLocationOnWorldEdge.y) - 0.5f), 0, maxY);

                mapData[xInMap, yInMap] = pixelDrawVal;
            }

            angle = (i / (Radius * PixelPerDistance));
            continentPixelProgress++;
        }

        return mapData;
    }

    public void Generate()
    {
        MapDisplay display = FindObjectOfType<MapDisplay>();

        rend = display.GetComponent<Renderer>();
        if (rend == null)
        {
            rend = display.gameObject.AddComponent<MeshRenderer>();
        }

        filter = display.GetComponent<MeshFilter>();
        if (filter == null)
        {
            filter = display.gameObject.AddComponent<MeshFilter>();
        }

        Seed = Random.Range(-100, 100);//temporary
        int pixelAmount = Mathf.RoundToInt(PixelPerDistance*(Radius*2)+1); //Calculate the total amount of pixels used to generate the map. +1 to centre create a centre for the circle
        var mapData = new float[pixelAmount, pixelAmount];

        new WorldData(pixelAmount, pixelAmount, rend.gameObject);
        for(int y = 0; y < pixelAmount; y ++)
        {
            for (int x = 0; x < pixelAmount; x++)
            {
                mapData[x, y] = 0f;
            }
        }
        UpdateRandomVarianceData();
        GenerateWorldSurfaceData();

        mapData = NoiseGenerator.GenerateNoiseMap(pixelAmount, pixelAmount, Seed, NoiseScale, Octaves, Persistance, Lacunarity, Offset);
        mapData = DrawContinentsOnMap(mapData);

        if (MapDrawMode == DrawType.Full)
            mapData = SquarizeWorldData(mapData, LandmassBlockSize);

        //create world data
        GenerateWorldDataFromHeightMap(mapData);

        var mesh = display.gameObject.GetComponentInChildren<MeshFilter>();
        if(mesh.sharedMesh == null)
            mesh.sharedMesh = GenerateCircleMesh(CircleSegmentCount, Radius);

        display.DrawMap(WorldData.instance, MapDrawMode);
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
                squarizedMap = WorldKernel.ApplySquareKernel(squarizedMap, x, y, kernelSize);
            }
        }

        return squarizedMap;
    }

    #region MeshGeneration
    private Mesh GenerateCircleMesh(int segments, float radius)
    {
        int CircleVertexCount = segments + 2;
        int CircleIndexCount = segments * 3;

        var circle = new Mesh();
        var vertices = new List<Vector3>(CircleVertexCount);
        var indices = new int[CircleIndexCount];
        List<Vector2> uvs = new List<Vector2>(CircleVertexCount);
        var segmentWidth = Mathf.PI * 2f / segments;
        var angle = 0f;
        vertices.Add(Vector3.zero);
        uvs.Add(Vector2.zero);
        for (int i = 1; i < CircleVertexCount; ++i)
        {
            vertices.Add(new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius);
            uvs.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
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
    #endregion
}

