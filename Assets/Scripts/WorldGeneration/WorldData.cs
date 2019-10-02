using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldData
{
    public static WorldData instance;

    private WorldNode[,] worldGrid;
    private int worldWidth;
    private int worldHeight;

    public WorldData(int _worldwidth, int _worldheight)
    {
        worldWidth = _worldwidth;
        worldHeight = _worldheight;

        Initialize();
    }

    public void Initialize()
    {
        instance = this;

        for (int y = 0; y < worldWidth; y++)
        {
            for (int x = 0; x < worldHeight; x++)
            {
                SetNodeDataInWorldGridAtPosition(new WorldNode(0, WorldNodeType.Invalid),new Vector2Int(x,y));
            }
        }
    }

    public void SetNodeDataInWorldGridAtPosition(WorldNode node, Vector2Int position)
    {
        //convert from position relative to center, to array coords


        worldGrid[position.x, position.y] = node;
    }

    public WorldNode GetNodeDataAtPosition(Vector2Int position)
    {
        //convert from position relative to center, to array coors

        return worldGrid[position.x, position.y];
    }

    public void FindLocationOnPlanet()//To Find Coordinates on planet surface - used for objects etc.
    {
        
    }

    public static void SetValueInDataMap(float[,] mapData, float newValue, Vector2 position)
    {
        var maxX = mapData.GetLength(0) - 1;
        var maxY = mapData.GetLength(1) - 1;

        var xInMap = Mathf.RoundToInt((((maxX + 1) / 2f) + position.x) - 0.5f);
        var yInMap = Mathf.RoundToInt((((maxY + 1) / 2f) + position.y) - 0.5f);

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
    }
}

public class WorldNode
{
    public Vector2Int RelativePosition = Vector2Int.zero; //Position Relative to world center
    public int NodeValue;
    public WorldNodeType nodeType;
    //normal


    public WorldNode(int _nodeValue, WorldNodeType _nodeType)
    {
        NodeValue = _nodeValue;
        nodeType = _nodeType;
    }

    public static WorldNode operator +(WorldNode a, int b)
        => new WorldNode(a.NodeValue + b, a.nodeType);

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