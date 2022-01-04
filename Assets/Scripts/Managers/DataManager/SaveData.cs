using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int levelAt = 0;
    public string hashValue;
    public bool[] levelCompleted;
    
    public SaveData()
    {
        levelAt = 0;
        levelCompleted = new bool [50];
    }
}
