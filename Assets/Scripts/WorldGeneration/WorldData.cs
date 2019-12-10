using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldData
{
    public static WorldData instance;

    public WorldNode[,] worldGrid;
    private int planetRadiusGrid;
    private float planetRadiusWorld;
    private float worldHeightInWorldUnits;
    public int worldHeight;

    public GameObject planetObject;


    public WorldData(float _worldheight, int _pixelsPerWorldUnit, int _planetRadius, GameObject _planetObject)
    {
        //rename these to be more obvious
        worldHeight = (int)_worldheight*_pixelsPerWorldUnit + 1; //add 1 to have an uneven number of units (such that there is a center in the world
        worldHeightInWorldUnits = _worldheight;

        planetRadiusGrid = _planetRadius;
        planetObject = _planetObject;
        planetRadiusWorld = _planetRadius / (float)_pixelsPerWorldUnit; //TODO: no need to calculate this.. it should be passed through in constructor.

        Initialize();
    }

    public void Initialize()
    {
        instance = this;
        worldGrid = new WorldNode[worldHeight, worldHeight];
        for (int y = 0; y < worldHeight; y++)
        {
            for (int x = 0; x < worldHeight; x++)
            {
                SetNodeDataAtGridPosition(new WorldNode(0, WorldNodeType.Invalid),new Vector2Int(x,y));
            }
        }
    }

    public void SetNodeDataAtGridPosition(WorldNode node, Vector2Int position)
    {
        node.RelativePosition = position;
        worldGrid[position.x, position.y] = node;
    }

    public WorldNode GetNodeDataInWorldAtGridPosition(Vector2Int position)
    {
        if (position.x < 0 || position.x >= worldHeight)
        {
            return new WorldNode(0, WorldNodeType.Invalid); //error
        }
        if (position.y < 0 || position.y >= worldHeight)
        {
            return new WorldNode(0, WorldNodeType.Invalid); //error
        }

        return worldGrid[position.x, position.y];
    }

    public WorldNode GetNodeDataAtWorldPosition(Vector2 worldPosition)
    {
        var gridPos = GetWorldGridPositionFromWorldPosition(worldPosition);

        var result = GetNodeDataInWorldAtGridPosition(gridPos);

        return result;
    }

    public Vector2Int GetWorldGridPositionFromWorldPosition(Vector2 worldPosition)
    {
        var halfHeight = Mathf.RoundToInt((worldGrid.GetLength(1) / 2));

        var NodesPerWorldUnitHeight = (worldHeight / worldHeightInWorldUnits);

        var x = ((planetObject.transform.position.x + worldPosition.x) * NodesPerWorldUnitHeight);
        var y = ((planetObject.transform.position.y + worldPosition.y) * NodesPerWorldUnitHeight);

        x = halfHeight + x;
        y = halfHeight + y;
        x++;
        y++;

        int xRes = (int)x;
        int yRes = (int)y;
        return new Vector2Int(xRes, yRes);
    }

    public Vector3 GetWorldPositionFromGridPosition(Vector2Int gridPosition) //might need more digits to be precise enough
    {
        var halfHeight = Mathf.RoundToInt((worldGrid.GetLength(1) / 2));

        var NodesPerWorldUnitHeight = (worldHeight / worldHeightInWorldUnits); // set this as a private property

        float newX = (float)(gridPosition.x-halfHeight) / NodesPerWorldUnitHeight;
        float newY = (float)(gridPosition.y-halfHeight) / NodesPerWorldUnitHeight;

        Vector3 newPosition = new Vector3(newX + planetObject.transform.position.x, newY + planetObject.transform.position.y, planetObject.transform.position.z);

        return newPosition;
    }

    public bool IsWorldGridPositionWithinPlanetArea(Vector2Int gridPosition)
    {
        Vector2 planetCenter2D = new Vector2(planetObject.transform.position.x, planetObject.transform.position.y);
        Vector2Int PlanetCenterInWorldGrid = GetWorldGridPositionFromWorldPosition(planetCenter2D);

        int dx = Mathf.Abs(gridPosition.x - PlanetCenterInWorldGrid.x);
        int dy = Mathf.Abs(gridPosition.y - PlanetCenterInWorldGrid.y);

        if (dx > planetRadiusGrid || dy > planetRadiusGrid)
        {
            return false;
        }

        if (dx + dy <= planetRadiusGrid)
        { 
            return true;
        }

        if ((dx * dx) + (dy * dy) < planetRadiusGrid * planetRadiusGrid)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public Vector3 GetClosestWorldPointOnPlanet(Vector3 worldPosition)
    {
        Vector2 pos = new Vector2(worldPosition.x, worldPosition.y);

        var planetVec = new Vector2(planetObject.transform.position.x, planetObject.transform.position.y);
        var toVector = (pos.normalized * planetRadiusWorld) - planetVec;

        var newRes = planetVec + toVector;

        Vector3 returnPos = new Vector3(newRes.x, newRes.y, worldPosition.z);

        return returnPos;
    }
}

public class WorldNode
{
    public Vector2Int? RelativePosition = null; //Position Relative to world center
    public float NodeHeight;
    public WorldNodeType nodeType;

    public WorldNode(float _nodeValue, WorldNodeType _nodeType)
    {
        nodeType = _nodeType;
        NodeHeight = (float)_nodeValue;
    }

    public static WorldNode operator +(WorldNode a, int b)
        => new WorldNode(a.NodeHeight + b, a.nodeType);

    public static WorldNode operator -(WorldNode a, int b)
        => a + (-b);
}

public enum WorldNodeType
{
    Land,
    Water,
    Air,
    Space,
    Invalid
}