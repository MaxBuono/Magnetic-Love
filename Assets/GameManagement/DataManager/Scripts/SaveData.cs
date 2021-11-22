using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int LevelAt = 2;
    public string hashValue;
    
    public SaveData()
    {
        LevelAt = 2;
    }
}
