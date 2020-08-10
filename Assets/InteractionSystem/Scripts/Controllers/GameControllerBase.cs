using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerBase : MonoBehaviour
{
    public static GameControllerBase instance;
    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
