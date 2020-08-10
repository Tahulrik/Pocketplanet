using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDebugLogger
{
    void Log(string message); //Implementations should always Call implementation in "DebugGameLogController"
}
