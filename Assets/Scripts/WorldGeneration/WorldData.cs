using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldData : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public struct PixelType
{
    public int pixelValue;
    public bool isLand;

    public PixelType(int _pixelValue, bool _isLand)
    {
        pixelValue = _pixelValue;
        isLand = _isLand;
    }

    public static PixelType operator +(PixelType a, int b)
        => new PixelType(a.pixelValue + b, a.isLand);

    public static PixelType operator -(PixelType a, int b)
        => a + (-b);
}