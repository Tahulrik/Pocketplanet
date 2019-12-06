using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldData
{
    public static WorldData instance;

    private WorldNode[,] worldGrid;
    public int worldWidth;
    public int worldHeight;

    private GameObject worldObject;

    private List<WorldNode> grid;

    public WorldData(int _worldwidth, int _worldheight, GameObject _worldObject)
    {
        worldWidth = _worldwidth;
        worldHeight = _worldheight;
        worldObject = _worldObject;

        Initialize();
    }

    public void Initialize()
    {
        instance = this;
        worldGrid = new WorldNode[worldWidth, worldHeight];
        for (int y = 0; y < worldWidth; y++)
        {
            for (int x = 0; x < worldHeight; x++)
            {
                SetNodeDataInWorldAtGridPosition(new WorldNode(0, WorldNodeType.Invalid),new Vector2Int(x,y));
            }
        }
    }

    public void SetNodeDataInWorldAtGridPosition(WorldNode node, Vector2Int position)
    {
        worldGrid[position.x, position.y] = node;
    }

    public WorldNode GetNodeDataInWorldAtGridPosition(Vector2Int position)
    {
        if (position.x < 0 || position.x > worldWidth)
        {
            return new WorldNode(0, WorldNodeType.Invalid); //error
        }
        if (position.y < 0 || position.y > worldHeight)
        {
            return new WorldNode(0, WorldNodeType.Invalid); //error
        }

        return worldGrid[position.x, position.y];
    }

    public WorldNode GetNodeDataAtWorldPosition(Vector2 worldPosition)
    {
        var halfWidth = Mathf.RoundToInt((worldGrid.GetLength(0) / 2));
        var halfHeight = Mathf.RoundToInt((worldGrid.GetLength(1) / 2));
        var NodesPerWorldUnitWidth = Mathf.RoundToInt(worldWidth / 10f);
        var NodesPerWorldUnitHeight = Mathf.RoundToInt(worldHeight / 10f);
        var x = (worldPosition.x * NodesPerWorldUnitWidth);
        var y = (worldPosition.y * NodesPerWorldUnitHeight);

        x = halfWidth + x;
        y = halfHeight + y;
        x++;
        y++;
        var result = GetNodeDataInWorldAtGridPosition(new Vector2Int((int)x, (int)y));

        return result;
    }

    public void FindLocationOnPlanet()//To Find Coordinates on planet surface - used for objects etc.
    {
        
    }
}

public class WorldNode
{
    public Vector2Int RelativePosition = Vector2Int.zero; //Position Relative to world center
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
    Underground, //Currently Unused
    Invalid
}