using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int LevelAt = 0;
    public string hashValue;
    
    public SaveData()
    {
        LevelAt = 0;
    }
}
