using System.Collections;
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
    public List<PixelType> _continentsCoverageData;

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
    void OnValidate()
    {
        _worldCircumference = (int)((2 * (Radius * PixelPerDistance)) * Mathf.PI);
        _continentsCoverageData = new List<PixelType>(Continents);
        _totalPixelsOnDiameter = (int)(Radius * 2) * PixelPerDistance;
        _pixelSize = (Radius) / (PixelPerDistance * _totalPixelsOnDiameter);
        _calculatedLandDepth = LandDepth * (int)Radius;
    }
    #endregion

    public void UpdateRandomVarianceData()
    {
        Continents = Random.Range(2, 5);
        _continentsCoverageData = new List<PixelType>(Continents);

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
                if (remainingWaterCoverage < minimumViableOceanSize)
                {
                    int missingAmount = minimumViableOceanSize - remainingWaterCoverage;

                    var selectedContinent = _continentsCoverageData.First(x => x.pixelValue >= missingAmount && !x.isLand);
                    selectedContinent -= missingAmount;
                    remainingWaterCoverage += missingAmount;
                }

                if (remainingLandCoverage < minimumViableLandSize)
                {
                    int missingAmount = minimumViableLandSize - remainingLandCoverage;

                    var selectedContinent = _continentsCoverageData.First(x => x.pixelValue >= missingAmount && x.isLand);
                    selectedContinent -= missingAmount;
                    remainingLandCoverage += missingAmount;
                }

                _continentsCoverageData.Add(new PixelType(remainingLandCoverage, true));
                _continentsCoverageData.Add(new PixelType(remainingWaterCoverage, false));
                break;
            }

            int newContinentSize = 0;
            int newOceanSize = 0;

            newContinentSize = Mathf.RoundToInt(totalPossibleLandCoverage / Continents) + continentSizeRandomVal;

            remainingLandCoverage -= newContinentSize;
            _continentsCoverageData.Add(new PixelType(newContinentSize, true));

            newOceanSize = remainingWaterCoverage / 4 + minimumViableLandSize;
            remainingWaterCoverage -= newOceanSize;
            _continentsCoverageData.Add(new PixelType(newOceanSize, false));
        }
    }

    public void Generate()
    {
        Seed = Random.Range(-100, 100);//temporary
        int pixelAmount = Mathf.RoundToInt(PixelPerDistance*(Radius*2)+1); //Calculate the total amount of pixels used to generate the map. +1 to centre create a centre for the circle
        var mapData = new float[pixelAmount, pixelAmount];

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

        MapDisplay display = FindObjectOfType<MapDisplay>();

        
        var mesh = display.gameObject.GetComponentInChildren<MeshFilter>();
        //if(mesh.sharedMesh == null)
            mesh.sharedMesh = GenerateCircleMesh(CircleSegmentCount, Radius);

        display.DrawNoiseMap(mapData, MapDrawMode);
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

    float[,] SetValueInDataMap(float[,] mapData, float newValue, Vector2 position)
    {
        var maxX = mapData.GetLength(0)-1;
        var maxY = mapData.GetLength(1)-1;

        var xInMap = Mathf.RoundToInt((((maxX+1)/2f) + position.x) - 0.5f);
        var yInMap = Mathf.RoundToInt((((maxY+1)/2f) + position.y) - 0.5f);

        if (xInMap < 0)
        {
            xInMap = 0;
        }
        if (xInMap > maxX)
        {
            xInMap = maxX;
        }
        if (yInMap < 0)
        {
            yInMap = 0;
        }
        if (yInMap > maxY)
        {
            yInMap = maxY;
        }

        mapData[xInMap, yInMap] = newValue;

        return mapData;
    }

    float[,] DrawContinentsOnMap(float[,] mapData)
    {
        float angle = 0f;

        PixelType currentDrawPixelAmount = _continentsCoverageData.First();
        int continentProgress = 0;
        int continentPixelProgress = 0;
        bool DrawingContinent = true;
        bool HasSelectedDrawType = true;
        float pixelDrawVal = 0;

        for (int i = 0 + ContinentsOffset; i <= _worldCircumference + ContinentsOffset; ++i)
        {
            if (currentDrawPixelAmount.pixelValue <= continentPixelProgress)
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
                if(continentProgress < _continentsCoverageData.Count)
                    currentDrawPixelAmount = _continentsCoverageData[continentProgress];
                continentPixelProgress = 0;
                HasSelectedDrawType = true;
            }

            for (int depth = -_calculatedLandDepth; depth <= _calculatedLandDepth; depth++)
            {
                var currentDist = Radius-(depth*_pixelSize);
                var pixelLocationOnWorldEdge = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle))
                    * (PixelPerDistance * currentDist);

                pixelDrawVal = (DrawingContinent) ? 1 : 0;
                SetValueInDataMap(mapData, pixelDrawVal, pixelLocationOnWorldEdge);
            }

            angle = (i/(Radius*PixelPerDistance));
            continentPixelProgress++;
        }

        return mapData;
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

