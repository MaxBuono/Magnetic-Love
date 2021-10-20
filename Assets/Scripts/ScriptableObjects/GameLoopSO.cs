using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GameLoop Manager", menuName = "GameLoop Manager")]
public class GameLoopSO : ScriptableObject
{
    public bool levelRunning = true;
    public bool isSplitted = true;
}
