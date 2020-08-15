using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGameController : MonoBehaviour, IDebugLogger
{
    public bool LogEvents = false;
    public static BaseGameController instance;
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

    public void Log(string message)
    {
        if (LogEvents)
        {
            string logFormattedMessage = $"{DateTime.Now.ToString()}: {message}";
            print(logFormattedMessage);
        }
    }
}
