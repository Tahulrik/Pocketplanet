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
    [Range(0.1f, 2f)]
    public float AtmosphereHeightPart = 0.8f;
    public AnimationCurve AtmospherFalloff;

    [Range(1, 300)]
    public int PixelPerDistance = 100;
    int _totalPixelsOnRadius;
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

            Debug.DrawLine(Camera.main.transform.position, screenPos, Color.blue, 5);

            var node = WorldData.instance.GetNodeDataAtWorldPosition(posInMapCoords);
            print($"inworldPos {posInMapCoords} and ingridpos {node.RelativePosition} is node of type: "+ node.nodeType.ToString());


        }
    }
    void OnValidate()
    {
        _continentsCoverageData = new List<int>(Continents);
        _totalPixelsOnRadius = (int)(Radius) * PixelPerDistance;
        _worldCircumference = (int)((2 * _totalPixelsOnRadius) * Mathf.PI);
        _pixelSize = (Radius) / (PixelPerDistance * _totalPixelsOnRadius);
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

    public void GenerateWorldContinentsData()
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

    void GenerateWorldData(float[,] heightMap)
    {
        int height = heightMap.GetLength(0);
        float heightInWorldSpaceUnits = height/PixelPerDistance;
        float atmosphereHeight = (int)(Radius * AtmosphereHeightPart);
        float spaceHeight = atmosphereHeight * 2;
        float totalMapHeight = heightInWorldSpaceUnits + (atmosphereHeight * 2) + (spaceHeight * 2);


        new WorldData(totalMapHeight, PixelPerDistance, _totalPixelsOnRadius, rend.gameObject);

        int totalMapHeightPixels = (int)totalMapHeight * PixelPerDistance;
        int planetOffset = (int)(atmosphereHeight + spaceHeight)*PixelPerDistance;

        for (int x = 0; x < totalMapHeightPixels; x++)//using height and height is fine, world grid should always be a square
        {
            for (int y = 0; y < totalMapHeightPixels; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                if (WorldData.instance.IsWorldGridPositionWithinPlanetArea(gridPos))
                {
                    int xInPlanet = x - planetOffset;
                    int yInPlanet = y - planetOffset;

                    //clamp values - probably rounding errors that creates this issue. Shouldnt ever become a problem.
                    //Consider refactoring some day
                    if (xInPlanet < 0)
                    {
                        xInPlanet = 0;
                    }
                    if (xInPlanet >= height)
                    {
                        xInPlanet = height-1; //sbutract 1 for index normalising
                    }
                    if (yInPlanet < 0)
                    {
                        yInPlanet = 0;
                    }
                    if (yInPlanet >= height)
                    {
                        yInPlanet = height-1; //subtract 1 for index normalising
                    }

                    var value = heightMap[xInPlanet, yInPlanet];
                    WorldNodeType newNodeType = (value < 1) ? WorldNodeType.Water : WorldNodeType.Land;
                    WorldNode newNode = new WorldNode(value, newNodeType);
                    WorldData.instance.SetNodeDataAtGridPosition(newNode, new Vector2Int(x, y));
                }
                else 
                {
                    var currentWorldPos = WorldData.instance.GetWorldPositionFromGridPosition(new Vector2Int(x, y));
                    var planetSurfacePoint = WorldData.instance.GetClosestWorldPointOnPlanet(currentWorldPos);

                    float altitude = Vector3.Distance(planetSurfacePoint.normalized, currentWorldPos.normalized);
                    var airVal = Mathf.Abs(AtmospherFalloff.Evaluate(altitude));

                    WorldNodeType newNodeType = WorldNodeType.Invalid;
                    if (airVal <= 0)
                        newNodeType = WorldNodeType.Space;
                    else
                        newNodeType = WorldNodeType.Air;

                    WorldNode newNode = new WorldNode(airVal, newNodeType);
                    WorldData.instance.SetNodeDataAtGridPosition(newNode, new Vector2Int(x, y));
                }
            }
        }
    }

    float[,] AddContinentsInMap(float[,] mapData)
    {
        float angle = 0f;

        int currentContinent = _continentsCoverageData.First();
        int continentProgress = 0;
        int continentPixelProgress = 0;
        bool AddingContinent = true;
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
                if (AddingContinent)
                {
                    AddingContinent = false;
                }
                else
                {
                    AddingContinent = true;
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

                pixelDrawVal = (AddingContinent) ? 1 : 0;

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

        
        //set size of background to match planet size

        for(int y = 0; y < pixelAmount; y ++)
        {
            for (int x = 0; x < pixelAmount; x++)
            {
                mapData[x, y] = 0f;
            }
        }
        UpdateRandomVarianceData();
        GenerateWorldContinentsData();

        mapData = NoiseGenerator.GenerateNoiseMap(pixelAmount, pixelAmount, Seed, NoiseScale, Octaves, Persistance, Lacunarity, Offset);
        mapData = AddContinentsInMap(mapData);

        if (MapDrawMode == DrawType.Full)
            mapData = SquarizeWorldData(mapData, LandmassBlockSize);

        //create world data
        GenerateWorldData(mapData);

        var mesh = display.gameObject.GetComponentInChildren<MeshFilter>();
        
        if(mesh.sharedMesh == null && mesh.mesh.bounds.extents.x != Radius)
            mesh.sharedMesh = GenerateCircleMesh(CircleSegmentCount, Radius);

        display.DrawMap(mapData, MapDrawMode);

        display.DrawSky(WorldData.instance.worldGrid);
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

